
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using Project.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Project.API.Applications.Service;
using Project.API.Applications.Queries;
using Project.Infrastructure.Repositories;
using Project.Domain.AggregatesModel;
using Project.API.Dtos;
using Consul;
using Project.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 添加这个
using Microsoft.IdentityModel.Tokens; // 添加这个
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using DotNetCore.CAP;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 明确指定包含 MediatR 处理程序的程序集
// MediatR 11.0 及更早版本的注册方式
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

//阿里云的数据库
//"MySQLProject": "Server=8.140.59.193;port=3307;Database=beta_project;User=abel;Password=acsdev312!@"

var connectionString = builder.Configuration.GetConnectionString("MySQLProject");
Console.WriteLine($"DB Server: {connectionString}");

builder.Services.AddInfrastructure(builder.Configuration); // Infrastructure 扩展

builder.Services.AddScoped<IRecommendService, TestRecommendService>();
builder.Services.AddScoped<IProjectQueries, ProjectQueries>(provider => new ProjectQueries(connectionString));
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

// Consul配置
builder.Services.Configure<ServerDiscoveryConfig>(builder.Configuration.GetSection("ServerDiscovery"));
builder.Services.AddSingleton<IConsulClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServerDiscoveryConfig>>().Value;
    return new ConsulClient(cfg => cfg.Address = new Uri(config.Consul.HttpEndpoint));
});
builder.Services.AddSingleton<ConsulRegistrationService>();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// 根据环境设置不同的 Authority
if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    // 生产环境：使用 Identity Server 容器名
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = "http://user-identity"; // 容器内部通信，使用 HTTP
            options.RequireHttpsMetadata = false; // 内部通信不需要 HTTPS
            options.Audience = "project_api";
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
        options.RequireHttpsMetadata = true; // 开发环境可以设为false
        options.Audience = "project_api";
        // options.TokenValidationParameters = new TokenValidationParameters
        // {
        //     ValidateIssuer = true,
        //     ValidIssuer = "https://localhost:5203"
        // };
    });
}

builder.Services.AddHealthChecks();

// 添加 CAP 配置（MySQL 版本）
builder.Services.AddCap(x =>
{
    x.UseEntityFramework<ProjectContext>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
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
