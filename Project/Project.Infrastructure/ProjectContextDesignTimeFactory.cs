using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;  // 添加这个
using Microsoft.Extensions.Configuration.Json;  // 添加这个
using Microsoft.EntityFrameworkCore;  // 添加这个
using System.IO;
namespace Project.Infrastructure
{
    public class ProjectContextDesignTimeFactory : IDesignTimeDbContextFactory<ProjectContext>
    {
        public ProjectContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Project.API"))
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("MySQLProject");

            var optionsBuilder = new DbContextOptionsBuilder<ProjectContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            // 设计时不需要 IMediator，传递 null
            return new ProjectContext(optionsBuilder.Options, null);
        }
    }
}