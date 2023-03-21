using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crosscutting.Configuration.AuthPolicyConfiguration;

public static class PolicyConfiguration
{
        public static IServiceCollection AddPolicyConfiguration(this IServiceCollection services) 
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("hwid", policy => policy.RequireClaim("hwid"));
                options.AddPolicy("staff", policy => policy.RequireClaim("staff"));
                options.AddPolicy("admin", policy => policy.RequireClaim("admin"));
            });

            return services;
        }
}