namespace HireHub.Contracts.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : notnull;
}
