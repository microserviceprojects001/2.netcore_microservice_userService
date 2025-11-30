using Microsoft.EntityFrameworkCore;
using Recommend.API.Data;
using Recommend.API.Dtos;
using Recommend.API.Infrastructure;
using Consul;
using Resilience;
using Microsoft.Extensions.Options;
using Recommend.API.Service;
using DotNetCore.CAP;
using Recommend.API.IntegrationEventHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("MySQLRecommends");
Console.WriteLine($"DB Server: {connectionString}");

builder.Services.AddDbContext<RecommendDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder.Services.Configure<ServerDiscoveryConfig>(builder.Configuration.GetSection("ServerDiscovery"));

builder.Services.AddSingleton<IConsulClient>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServerDiscoveryConfig>>().Value;
    return new ConsulClient(cfg => cfg.Address = new Uri(config.Consul.HttpEndpoint));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(typeof(ResilienceClientFactory), sp =>
{
    var logger = sp.GetRequiredService<ILogger<ResilienceHttplicent>>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var retryCount = 5;
    var exceptionCountAllowedBeforeBreaking = 3;

    return new ResilienceClientFactory(logger, httpContextAccessor, retryCount, exceptionCountAllowedBeforeBreaking);
});
builder.Services.AddSingleton<IHttpClient>(sp =>
{

    return sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttplicent();
});
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContactService, ContactService>();

// 添加 CAP 配置（MySQL 版本）
builder.Services.AddCap(x =>
{
    x.UseEntityFramework<RecommendDBContext>();
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
builder.Services.AddScoped<ProjectCreatedIntegrationEventHandler>();
builder.Services.AddScoped<IInternalAuthService, InternalAuthService>();
builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("ClientSettings"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
