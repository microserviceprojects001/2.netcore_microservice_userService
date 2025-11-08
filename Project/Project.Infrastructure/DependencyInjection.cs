
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Infrastructure;
using Microsoft.EntityFrameworkCore;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("MySQLProject");

        services.AddDbContext<ProjectContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                mysqlOptions =>
                {
                    mysqlOptions.MigrationsAssembly(typeof(ProjectContext).Assembly.FullName);
                });
        });

        return services;
    }
}
