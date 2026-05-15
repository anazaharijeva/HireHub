using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace HireHub.EventBus;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqOptions _options;
    private readonly object _gate = new();
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            EnsureChannel();
            var routingKey = typeof(T).Name;
            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            var props = _channel!.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;

            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body);
        }

        _logger.LogDebug("Published {RoutingKey}", typeof(T).Name);
        return Task.CompletedTask;
    }

    private void EnsureChannel()
    {
        if (_channel is { IsOpen: true })
            return;

        _channel?.Dispose();
        _connection?.Dispose();

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _channel?.Dispose();
            _channel = null;
            _connection?.Dispose();
            _connection = null;
        }
    }
}
