using Microsoft.Extensions.DependencyInjection;

namespace Crosscutting.Configuration.AuthPolicyConfiguration;

public static class PolicyConfiguration
{
    public static IServiceCollection AddClaimPolicyConfiguration(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("user", policy => policy.RequireClaim("user"));
            options.AddPolicy("hwid", policy => policy.RequireClaim("hwid"));
            options.AddPolicy("staff", policy => policy.RequireClaim("staff"));
            options.AddPolicy("admin", policy => policy.RequireClaim("admin"));
        });

        return services;
    }
}