using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// 添加 OpenAPI（只在开发环境使用）
// 获取当前环境
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                  builder.Environment.EnvironmentName ?? "Development";

if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddOpenApi();
}

// 根据环境配置认证
if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    // 生产环境配置
    builder.Services.AddAuthentication()
        .AddJwtBearer("finbook", options =>
        {
            //http://user-identity 等价于 http://user-identity:80

            options.Authority = "http://user-identity";  // 使用容器名
            options.RequireHttpsMetadata = false;  // 内部通信不需要 HTTPS
            options.Audience = "gateway_api";

            // 生产环境下可能需要调整安全设置
            //options.TokenValidationParameters.ValidateIssuer = true;
            //options.TokenValidationParameters.ValidIssuer = "http://user-identity";

            // 如果使用自签名证书，可以这样配置（不推荐生产环境使用）
            // if (builder.Configuration["Authentication:AllowInsecure"] == "true")
            // {
            //     options.BackchannelHttpHandler = new HttpClientHandler
            //     {
            //         ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            //     };
            // }
        });

    Console.WriteLine("已配置生产环境认证");
}
else
{
    // 开发环境配置
    builder.Services.AddAuthentication()
        .AddJwtBearer("finbook", options =>
        {
            options.Authority = "https://localhost:5202";
            options.RequireHttpsMetadata = true;
            options.Audience = "gateway_api";
        });

    Console.WriteLine("已配置开发环境认证");
}

builder.Services.AddOcelot();



// 输出当前环境，便于调试
Console.WriteLine($"当前运行环境: {environment}");

// 根据环境加载不同的 Ocelot 配置文件
if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    // 生产环境
    builder.Configuration
        .AddJsonFile("Ocelot.Production.json", optional: false, reloadOnChange: true);
    Console.WriteLine("已加载生产环境配置: Ocelot.Production.json");
}
else
{
    // 开发环境
    builder.Configuration
        .AddJsonFile("Ocelot.json", optional: false, reloadOnChange: true);
    Console.WriteLine("已加载开发环境配置: Ocelot.json");

    // 也可以加载开发环境特定的配置文件
    builder.Configuration.AddJsonFile($"Ocelot.{environment}.json", optional: true, reloadOnChange: true);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseOcelot();



app.Run();

