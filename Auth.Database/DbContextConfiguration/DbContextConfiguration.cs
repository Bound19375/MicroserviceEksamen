using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Database.DbContextConfiguration;

public static class DbContextConfiguration
{
    public static IServiceCollection AddMasterDbContext(this IServiceCollection services, IConfiguration builder)
    {
        // Add services to the container.
        //dotnet ef migrations add 0.1 --project Auth.Database --startup-project EFCore --context AuthDbContext
        //dotnet ef database update --project Auth.Database --startup-project EFCore --context AuthDbContext
        //dotnet tool update --global dotnet-ef
        //$env:ConnectionStrings__BoundcoreMaster='server=localhost;port=3306;database=Boundcore.Master;user=user;password=password;AllowPublicKeyRetrieval=True;SslMode=preferred;'

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseMySql(builder.GetConnectionString("BoundcoreMaster") ?? throw new InvalidOperationException("Version Exception"),
                ServerVersion.AutoDetect(builder.GetConnectionString("BoundcoreMaster")) ?? throw new InvalidOperationException("ConnStr Exception"),
                x =>
                {

                });
        });

        return services;
    }
}