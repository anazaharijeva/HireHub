using ApplicationService.Application.Abstractions;
using ApplicationService.Infrastructure.Messaging;
using ApplicationService.Infrastructure.Persistence;
using HireHub.Contracts.Messaging;
using HireHub.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddRabbitMqPublisher();
        services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ApplicationDb")));

        services.AddScoped<IApplicationsRepository, ApplicationsRepository>();
        return services;
    }

    public static async Task EnsureApplicationDatabaseAsync(this IServiceProvider sp, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);
    }
}
