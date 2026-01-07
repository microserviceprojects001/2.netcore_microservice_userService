using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Models;
using User.API.Filters;
using Consul;
using User.API.Dtos;
using Microsoft.Extensions.Options;
using User.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 添加这个
using Microsoft.IdentityModel.Tokens; // 添加这个
using System.IdentityModel.Tokens.Jwt;
using DotNetCore.CAP;
using Resilience.ZipkinExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
}).AddNewtonsoftJson();

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("MySQL");
Console.WriteLine($"DB Server: {connectionString}");

builder.Services.AddDbContext<UserContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Consul配置
builder.Services.Configure<ServerDiscoveryConfig>(builder.Configuration.GetSection("ServerDiscovery"));
builder.Services.AddSingleton<IConsulClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServerDiscoveryConfig>>().Value;
    return new ConsulClient(cfg => cfg.Address = new Uri(config.Consul.HttpEndpoint));
});

// 修改为使用应用生命周期事件注册
builder.Services.AddSingleton<ConsulRegistrationService>();
// 移除之前的 HostedService 注册

builder.Services.AddHealthChecks();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

// 根据环境设置不同的 Authority
if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    // 生产环境：使用 Identity Server 容器名
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = "http://user-identity"; // 容器内部通信，使用 HTTP
            options.RequireHttpsMetadata = false; // 内部通信不需要 HTTPS
            options.Audience = "user_api";
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidIssuer = "http://user-identity";
        });
}
else
{
    // 开发环境：使用网关地址
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = "https://localhost:5203"; // 网关地址
            options.RequireHttpsMetadata = true;
            options.Audience = "user_api";
        });
}

builder.Services.AddAuthorization(options =>
{
    //编译阶段就加载执行了
    // options.AddPolicy("RequireUserApiScope", policy =>
    //     {
    //         policy.RequireAuthenticatedUser();
    //         policy.RequireClaim("scope", "user_api");
    //     });
    options.AddPolicy("user_api", policy =>
      policy.RequireAssertion(context =>
      {
          var audienceClaims = context.User.FindAll(c => c.Type == "aud");
          return audienceClaims.Any(c => c.Value == "user_api");
      }));

    options.AddPolicy("RequireInternalAccess", policy =>
    policy.RequireAssertion(context =>
    {
        // 检查scope
        var scopeClaims = context.User.FindAll(c => c.Type == "scope");
        var userScopes = scopeClaims.SelectMany(c => c.Value.Split(' ')).ToList();
        if (!userScopes.Contains("user_api.internal"))
            return false;

        // 检查客户端类型
        return context.User.HasClaim("client_client_type", "microservice");
    }));
});

// 添加 CAP 配置（MySQL 版本）
builder.Services.AddCap(x =>
{
    x.UseEntityFramework<UserContext>();
    var configuration = builder.Configuration;
    x.UseRabbitMQ(option =>
    {
        option.HostName = configuration["RabbitMQ:HostName"];
        option.Port = int.Parse(configuration["RabbitMQ:Port"]);
        option.UserName = configuration["RabbitMQ:UserName"];
        option.Password = configuration["RabbitMQ:Password"];
        option.VirtualHost = configuration["RabbitMQ:VirtualHost"];

    });
    x.FailedRetryCount = 3;
    x.FailedRetryInterval = 60;
    x.UseDashboard(opt =>
    {
        opt.PathMatch = "/cap"; // Dashboard访问路径
    });
});

builder.Services.AddZipkinTracing(builder.Configuration);
var app = builder.Build();

// 初始化数据库
UserContextSeed.Seed(app);

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 只在非生产环境启用 HTTPS 重定向
if (!environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/HealthCheck");

// 使用应用生命周期事件触发注册
//ASP.NET Core 在创建主机时自动注册了 IHostApplicationLifetime
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var consulService = app.Services.GetRequiredService<ConsulRegistrationService>();

lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        Console.WriteLine("应用已完全启动");
        await consulService.RegisterAsync(lifetime.ApplicationStopping);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Consul 注册失败");
    }
});

lifetime.ApplicationStopping.Register(async () =>
{
    try
    {
        Console.WriteLine("应用正在停止...");
        await consulService.DeregisterAsync(lifetime.ApplicationStopping);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Consul 注销失败");
    }
});

app.Run();