using Microsoft.Extensions.DependencyInjection;

namespace HireHub.EventBus;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqPublisher(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        return services;
    }
}
