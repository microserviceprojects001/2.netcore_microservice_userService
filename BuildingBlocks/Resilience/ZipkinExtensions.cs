using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics; // 添加这个命名空间

namespace Resilience.ZipkinExtensions
{
    public static class ZipkinExtensions
    {
        /// <summary>
        /// 添加Zipkin分布式跟踪服务
        /// </summary>
        public static IServiceCollection AddZipkinTracing(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 从配置中获取设置
            var zipkinEndpoint = configuration["Zipkin:Endpoint"]
                ?? "http://8.140.59.193:9411/api/v2/spans";

            var serviceName = configuration["Zipkin:ServiceName"]
                ?? configuration.GetValue<string>("ApplicationName", "MyMicroservice");

            var debugMode = configuration.GetValue<bool>("Zipkin:Debug", false);

            // 控制台输出配置信息
            Console.WriteLine($"[Zipkin] 正在为服务 '{serviceName}' 配置分布式跟踪");
            Console.WriteLine($"[Zipkin] 数据上报地址: {zipkinEndpoint}");
            Console.WriteLine($"[Zipkin] 调试模式: {debugMode}");

            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    // 1. 设置服务资源信息
                    tracerProviderBuilder.SetResourceBuilder(
                            ResourceBuilder.CreateDefault().AddService(serviceName: serviceName));

                    // 2. 设置采样器（关键！确保所有请求都被跟踪）
                    tracerProviderBuilder.SetSampler(new AlwaysOnSampler());

                    // 3. 自动收集AspNetCore的请求跟踪
                    tracerProviderBuilder.AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;

                        // 添加调试信息
                        if (debugMode)
                        {
                            // 使用 EnrichWithHttpRequest 替代 Enrich
                            options.EnrichWithHttpRequest = (activity, httpRequest) =>
                            {
                                Console.WriteLine($"[AspNetCore] 开始跟踪: {activity.DisplayName}");
                                Console.WriteLine($"[AspNetCore] 请求路径: {httpRequest.Path}");
                                Console.WriteLine($"[AspNetCore] 当前TraceId: {activity.TraceId}");
                            };

                            // 也可以添加响应时的调试信息
                            options.EnrichWithHttpResponse = (activity, httpResponse) =>
                            {
                                Console.WriteLine($"[AspNetCore] 响应状态码: {httpResponse.StatusCode}");
                            };
                        }
                    });

                    // 4. 自动收集HttpClient发出的请求跟踪
                    tracerProviderBuilder.AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;

                        if (debugMode)
                        {
                            // 使用 EnrichWithHttpRequestMessage 和 EnrichWithHttpResponseMessage
                            options.EnrichWithHttpRequestMessage = (activity, request) =>
                            {
                                Console.WriteLine($"[HttpClient] 发送请求到: {request.RequestUri}");
                                Console.WriteLine($"[HttpClient] 当前TraceId: {activity.TraceId}");

                                // 检查是否包含跟踪头
                                var hasTraceHeader = request.Headers.Contains("traceparent");
                                Console.WriteLine($"[HttpClient] 请求头中包含traceparent: {hasTraceHeader}");
                            };

                            options.EnrichWithHttpResponseMessage = (activity, response) =>
                            {
                                Console.WriteLine($"[HttpClient] 收到响应状态码: {response.StatusCode}");
                            };
                        }
                    });

                    // 5. 添加Zipkin导出器
                    tracerProviderBuilder.AddZipkinExporter(zipkinOptions =>
                    {
                        zipkinOptions.Endpoint = new Uri(zipkinEndpoint);
                        zipkinOptions.UseShortTraceIds = true;

                        if (debugMode)
                        {
                            Console.WriteLine($"[Zipkin] 已配置导出器到: {zipkinEndpoint}");
                        }
                    });

                    // 6. 添加控制台导出器（用于调试）
                    if (configuration.GetValue<bool>("Zipkin:EnableConsoleExporter", false))
                    {
                        tracerProviderBuilder.AddConsoleExporter();
                        Console.WriteLine("[Zipkin] 已启用控制台导出器");
                    }
                });

            return services;
        }
    }
}