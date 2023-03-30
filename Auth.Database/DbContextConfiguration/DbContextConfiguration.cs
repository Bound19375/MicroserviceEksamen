using Crosscutting.TransactionHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Auth.Database.DbContextConfiguration;

public static class DbContextConfiguration
{
    public static IServiceCollection AddMasterDbContext(this IServiceCollection services, IConfiguration builder)
    {
        // Add services to the container.
        //dotnet ef migrations add 0.1 --project Auth.Database --startup-project BoundCoreWebApplication --context AuthDbContext
        //dotnet ef database update --project Auth.Database --startup-project BoundCoreWebApplication --context AuthDbContext
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseMySql(builder.GetConnectionString("BoundcoreMaster") ?? throw new InvalidOperationException(),
                ServerVersion.AutoDetect(builder.GetConnectionString("BoundcoreMaster")) ?? throw new InvalidOperationException(),
                x =>
                {

                });
        });

        return services;
    }
}