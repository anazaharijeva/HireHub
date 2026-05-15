using HireHub.Contracts.Messaging;
using HireHub.EventBus;

namespace UserService.Infrastructure.Messaging;

public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IRabbitMqPublisher _publisher;

    public RabbitMqIntegrationEventPublisher(IRabbitMqPublisher publisher) => _publisher = publisher;

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : notnull =>
        _publisher.PublishAsync(@event, cancellationToken);
}
