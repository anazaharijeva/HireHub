using HireHub.ApiCommon;
using HireHub.Contracts.Messaging;
using HireHub.EventBus;
using MessagingService.Infrastructure.Messaging;
using MessagingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessagingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMessagingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddRabbitMqPublisher();
        services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        services.AddDbContext<MessagingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MessagingDb")));

        services.AddScoped<IMessagingService, MessagingAppService>();
        return services;
    }

    public static async Task EnsureMessagingDatabaseAsync(this IServiceProvider sp, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        await db.Database.EnsureCreatedSafeAsync(ct).ConfigureAwait(false);
    }
}
