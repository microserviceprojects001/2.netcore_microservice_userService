
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 明确指定包含 MediatR 处理程序的程序集
// MediatR 11.0 及更早版本的注册方式
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

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

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

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
