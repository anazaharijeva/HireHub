using AuthService.Application.Abstractions;
using AuthService.Application.Options;
using AuthService.Application.Security;
using AuthService.Application.Services;
using AuthService.Infrastructure.Messaging;
using AuthService.Infrastructure.Persistence;
using HireHub.Contracts.Messaging;
using HireHub.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddRabbitMqPublisher();
        services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AuthDb")));

        services.AddScoped<IAuthDb>(sp => sp.GetRequiredService<AuthDbContext>());
        services.AddScoped<IAuthAccountService, AuthAccountService>();
        services.AddSingleton<IJwtTokenFactory, JwtTokenFactory>();

        return services;
    }

    public static async Task EnsureAuthDatabaseAsync(this IServiceProvider sp, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);
        if (await db.Set<AuthService.Domain.Entities.AppRole>().AnyAsync(ct).ConfigureAwait(false))
            return;

        foreach (var name in AuthService.Domain.Roles.All)
            db.Set<AuthService.Domain.Entities.AppRole>().Add(new AuthService.Domain.Entities.AppRole { Name = name });

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
