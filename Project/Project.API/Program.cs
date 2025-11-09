
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using Project.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
