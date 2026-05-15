using HireHub.Contracts.Messaging;
using HireHub.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Abstractions;
using UserService.Application.Services;
using UserService.Infrastructure.Messaging;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddRabbitMqPublisher();
        services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("UserDb")));

        services.AddScoped<IUserDb>(sp => sp.GetRequiredService<UserDbContext>());
        services.AddScoped<IUserProfileService, UserProfileService>();
        return services;
    }

    public static async Task EnsureUserDatabaseAsync(this IServiceProvider sp, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await db.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);
    }
}
