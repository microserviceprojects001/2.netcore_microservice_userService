using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Reflection;
using Serilog.Core;
using Microsoft.AspNetCore.Http;

namespace Microservices.Common.Logging;

/// <summary>
/// 微服务通用 Serilog 配置类
/// 专门用于与 Fluentd + Elasticsearch 架构配合
/// 
/// 设计理念：
/// 1. 开发环境：双重输出（控制台JSON + 本地文件文本），便于调试和模拟生产
/// 2. 生产环境：单输出（控制台JSON），便于容器化部署和Fluentd收集
/// 3. 统一日志结构：JSON格式，自动添加上下文信息，支持分布式追踪
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// 配置 Serilog（推荐使用方式）
    /// 在 Program.cs 中通过 builder.Host.UseMicroservicesSerilog() 调用
    /// </summary>
    /// <param name="hostBuilder">HostBuilder</param>
    /// <param name="applicationName">应用名称</param>
    /// <param name="environment">环境名称，如果为 null 则从 HostingEnvironment 获取</param>
    /// <returns>配置后的 HostBuilder</returns>
    public static IHostBuilder UseMicroservicesSerilog(
        this IHostBuilder hostBuilder,
        string applicationName,
        string environment = null)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            ConfigureSerilog(context, services, configuration, applicationName, environment);
        });
    }

    /// <summary>
    /// Serilog 配置核心方法
    /// 执行实际配置，包括：
    /// 1. 读取配置文件（appsettings.json）
    /// 2. 注册依赖注入服务
    /// 3. 添加上下文丰富器
    /// 4. 根据环境应用不同的输出配置
    /// </summary>
    public static void ConfigureSerilog(
        HostBuilderContext context,
        IServiceProvider services,
        LoggerConfiguration loggerConfiguration,
        string applicationName,
        string environment = null)
    {
        if (string.IsNullOrEmpty(environment))
        {
            environment = context.HostingEnvironment.EnvironmentName;
        }

        // 获取应用程序信息，用于丰富日志上下文
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? applicationName;
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";

        // 基础配置 - 从配置文件读取，并添加通用上下文信息
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)  // 从 appsettings.json 读取配置
            .ReadFrom.Services(services)                    // 从依赖注入容器读取配置
            .Enrich.FromLogContext()                        // 允许动态添加日志属性
            .Enrich.WithProperty("Application", applicationName)   // 应用名称
            .Enrich.WithProperty("Assembly", assemblyName)         // 程序集名称
            .Enrich.WithProperty("Version", assemblyVersion)       // 应用版本
            .Enrich.WithProperty("Environment", environment)       // 运行环境
            .Enrich.WithMachineName()                              // 机器名（用于多实例区分）
            .Enrich.WithThreadId()                                 // 线程ID（用于异步跟踪）
            .Enrich.WithCorrelationId()                            // 关联ID（分布式追踪关键）
            .Enrich.WithRequestContext();                          // 请求上下文信息

        // 根据环境配置不同的输出策略
        // 开发环境：详细日志，双重输出（控制台+文件）
        // 生产环境：精简日志，单输出（控制台JSON，供Fluentd收集）
        if (context.HostingEnvironment.IsDevelopment())
        {
            ConfigureDevelopment(loggerConfiguration);
        }
        else
        {
            ConfigureProduction(loggerConfiguration);
        }
    }

    /// <summary>
    /// 开发环境配置 - 输出详细日志到控制台和文件
    /// 
    /// 输出目标1：控制台（JSON格式）
    ///   - 目的：模拟生产环境输出，提前发现格式问题
    ///   - 格式：JSON，便于验证Fluentd是否能正确解析
    /// 
    /// 输出目标2：本地文件（文本格式）
    ///   - 目的：开发人员本地调试查看
    ///   - 格式：可读性强的文本，包含完整上下文信息
    ///   - 路径：logs/log-.txt（按天滚动，保留7天）
    /// 
    /// 日志级别：Debug及以上（所有详细信息）
    /// 第三方日志：Microsoft(Information)、System(Warning)以上
    /// </summary>
    private static void ConfigureDevelopment(LoggerConfiguration loggerConfiguration)
    {
        // 定义详细的文本输出模板，包含所有重要上下文信息
        var detailedOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
                              "[App: {Application}] " +
                              "[Source: {SourceContext}] " +
                              "[Action: {ActionName}] " +
                              "{Message:lj}{NewLine}{Exception}";

        loggerConfiguration
            .MinimumLevel.Debug()                                      // 记录所有Debug级别及以上的日志
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // 降低Microsoft命名空间的日志级别
            .MinimumLevel.Override("System", LogEventLevel.Warning)    // 降低System命名空间的日志级别
            .WriteTo.Console(new JsonFormatter(renderMessage: true))   // 控制台输出JSON（测试生产环境格式）
            .WriteTo.File(
                path: "logs/log-.txt",                                 // 文件路径，logs文件夹下的日志文件
                rollingInterval: RollingInterval.Day,                  // 按天滚动：每天生成新文件
                retainedFileCountLimit: 7,                             // 保留最近7天的日志文件
                shared: true,                                          // 允许多个进程写入同一文件
                outputTemplate: detailedOutputTemplate);               // 包含详细上下文信息的文本格式
    }

    /// <summary>
    /// 生产环境配置 - 只输出重要日志到控制台（JSON格式供Fluentd收集）
    /// 
    /// 输出目标：仅控制台（JSON格式）
    ///   - 目的：容器化部署时，Docker收集stdout输出
    ///   - 格式：JSON，便于Fluentd解析并转发到Elasticsearch
    /// 
    /// 日志级别：Information及以上（避免过多调试日志）
    /// 第三方日志：Microsoft(Warning)、System(Error)以上（减少噪音）
    /// 性能优化：只输出必要信息，减少I/O开销
    /// </summary>
    private static void ConfigureProduction(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .MinimumLevel.Information()                               // 仅记录Information级别及以上的日志
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // 提高Microsoft命名空间的日志级别，减少噪音
            .MinimumLevel.Override("System", LogEventLevel.Error)     // 提高System命名空间的日志级别，减少噪音
                                                                      // 关键：输出为JSON格式，便于Fluentd解析
            .WriteTo.Console(new JsonFormatter(renderMessage: true));  // 仅输出到控制台（容器stdout）
    }

    /// <summary>
    /// 创建引导记录器（用于 Program.cs 开头的配置）
    /// 
    /// 作用：在ASP.NET Core主机完全构建之前就能记录日志
    /// 关键：确保应用程序启动过程中（特别是依赖注入和配置加载阶段）发生的错误能被记录
    /// 特点：简化配置，只输出到控制台，使用JSON格式
    /// 
    /// 使用方式：
    ///   Program.cs第一行：Log.Logger = SerilogConfiguration.CreateBootstrapLogger("应用名");
    ///   Program.cs最后：Log.CloseAndFlush();
    /// </summary>
    public static ILogger CreateBootstrapLogger(string applicationName)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // 降低Microsoft日志级别，便于调试启动问题
            .Enrich.WithProperty("Application", applicationName)           // 添加应用名称标识
            .WriteTo.Console(new JsonFormatter())                          // 仅输出到控制台（JSON格式）
            .CreateBootstrapLogger();                                      // 创建引导记录器（不替换默认日志提供程序）
    }
}

/// <summary>
/// 关联 ID 丰富器 - 用于请求追踪
/// 
/// 作用：为每条日志添加唯一的关联ID，用于分布式系统追踪
/// 原理：
///   1. 优先使用 System.Diagnostics.Activity.Current?.Id（基于W3C Trace Context标准）
///   2. 如果没有Activity，使用RootId
///   3. 都没有则生成新的GUID
/// 
/// 在微服务架构中的重要性：
///   用户请求 → API网关 → 服务A → 服务B
///   所有相关服务日志都有相同的CorrelationId，可以在ELK/Kibana中追踪完整请求链
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // 获取关联ID：优先使用Activity追踪ID，其次RootId，都没有则生成新GUID
        var correlationId = System.Diagnostics.Activity.Current?.Id ??
                           System.Diagnostics.Activity.Current?.RootId ??
                           Guid.NewGuid().ToString();

        // 将CorrelationId添加到日志属性中
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
    }
}

/// <summary>
/// 请求上下文丰富器 - 记录基本的请求信息
/// 简化版：不依赖 IHttpContextAccessor，避免复杂的依赖注入问题
/// 
/// 设计理念：
///   1. 微服务中，详细的HTTP信息（路径、方法等）通常由API网关记录
///   2. 服务内部主要需要RequestId用于追踪
///   3. 避免依赖注入复杂性，保持简单稳定
/// 
/// 注意：如果确实需要完整HTTP上下文，建议使用专门的日志中间件或在控制器中手动添加
/// </summary>
public class RequestContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // 获取请求ID（与CorrelationId相同，但字段名不同，便于不同系统的兼容）
        var requestId = System.Diagnostics.Activity.Current?.Id ??
                       System.Diagnostics.Activity.Current?.RootId ??
                       Guid.NewGuid().ToString();

        // 添加RequestId到日志属性
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", requestId));
    }
}

/// <summary>
/// 扩展方法，用于丰富日志上下文
/// 
/// 作用：提供流畅的API，便于在LoggerConfiguration中链式调用
/// 例如：.Enrich.WithCorrelationId().Enrich.WithRequestContext()
/// </summary>
public static class LogContextEnrichers
{
    /// <summary>
    /// 添加关联ID丰富器
    /// 要求：CorrelationIdEnricher必须有公共无参构造函数
    /// </summary>
    public static LoggerConfiguration WithCorrelationId(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<CorrelationIdEnricher>();
    }

    /// <summary>
    /// 添加请求上下文丰富器
    /// 要求：RequestContextEnricher必须有公共无参构造函数
    /// </summary>
    public static LoggerConfiguration WithRequestContext(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<RequestContextEnricher>();
    }
}