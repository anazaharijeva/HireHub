using HireHub.ApiCommon;
using HireHub.Contracts.Messaging;
using HireHub.EventBus;
using JobService.Application.Abstractions;
using JobService.Application.Services;
using JobService.Infrastructure.Messaging;
using JobService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddJobInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddRabbitMqPublisher();
        services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        services.AddDbContext<JobDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("JobDb")));

        services.AddScoped<IJobDb>(sp => sp.GetRequiredService<JobDbContext>());
        services.AddScoped<IJobPostingService, JobPostingService>();
        return services;
    }

    public static async Task EnsureJobDatabaseAsync(this IServiceProvider sp, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobDbContext>();
        await db.Database.EnsureCreatedSafeAsync(ct).ConfigureAwait(false);
        if (await db.Set<JobService.Domain.Entities.JobCategory>().AnyAsync(ct).ConfigureAwait(false))
            return;

        var seed = new[] { "Engineering", "Design", "Product", "Sales", "HR" };
        foreach (var name in seed)
            db.Set<JobService.Domain.Entities.JobCategory>().Add(new JobService.Domain.Entities.JobCategory { Name = name });

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
