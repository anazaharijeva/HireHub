namespace HireHub.EventBus;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : notnull;
}
