using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HireHub.ApiCommon;

public static class HireHubCorsExtensions
{
    public static IServiceCollection AddHireHubDevCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
        });
        return services;
    }

    public static WebApplication UseHireHubDevCors(this WebApplication app)
    {
        app.UseCors();
        return app;
    }
}
