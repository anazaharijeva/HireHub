using System.Text;
using System.Text.Json;
using HireHub.Contracts.Events;
using HireHub.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Infrastructure.Messaging;

public sealed class IntegrationEventsWorker : BackgroundService
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<IntegrationEventsWorker> _logger;

    public IntegrationEventsWorker(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<IntegrationEventsWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Run(() => RunConsumer(stoppingToken), stoppingToken).ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ not ready; retrying in 5s");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private void RunConsumer(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        var queue = channel.QueueDeclare(queue: "hirehub.notifications", durable: true, exclusive: false, autoDelete: false).QueueName;
        channel.QueueBind(queue: queue, exchange: _options.ExchangeName, routingKey: "#");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                Handle(ea.RoutingKey ?? "", json).GetAwaiter().GetResult();
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process integration event");
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(queue, autoAck: false, consumer);
        while (!stoppingToken.IsCancellationRequested)
            Thread.Sleep(200);
    }

    private async Task Handle(string routingKey, string json)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        switch (routingKey)
        {
            case nameof(UserRegisteredEvent):
                var ur = JsonSerializer.Deserialize<UserRegisteredEvent>(json, Json);
                if (ur is not null)
                    await Add(db, ur.UserId, "Welcome", $"Account created for {ur.Email}", "user.registered").ConfigureAwait(false);
                break;
            case nameof(JobCreatedEvent):
                var jc = JsonSerializer.Deserialize<JobCreatedEvent>(json, Json);
                if (jc is not null)
                    await Add(db, jc.CreatorUserId, "Job published", jc.Title, "job.created").ConfigureAwait(false);
                break;
            case nameof(ApplicationCreatedEvent):
                var ac = JsonSerializer.Deserialize<ApplicationCreatedEvent>(json, Json);
                if (ac is not null)
                {
                    if (ac.RecruiterUserId is not null)
                        await Add(db, ac.RecruiterUserId.Value, "New application", $"Candidate applied to job {ac.JobId}", "application.created").ConfigureAwait(false);
                    await Add(db, ac.CandidateUserId, "Application sent", $"Applied to job {ac.JobId}", "application.created").ConfigureAwait(false);
                }

                break;
            case nameof(ApplicationUpdatedEvent):
                var au = JsonSerializer.Deserialize<ApplicationUpdatedEvent>(json, Json);
                if (au is not null)
                    await Add(db, au.CandidateUserId, "Application update", $"Status: {au.NewStatus}", "application.updated").ConfigureAwait(false);
                break;
            case nameof(MessageSentEvent):
                var ms = JsonSerializer.Deserialize<MessageSentEvent>(json, Json);
                if (ms is not null)
                    await Add(db, ms.ToUserId, "New message", ms.Preview, "message.sent").ConfigureAwait(false);
                break;
            default:
                _logger.LogInformation("Unhandled routing key {Key}", routingKey);
                break;
        }
    }

    private static async Task Add(NotificationDbContext db, Guid userId, string title, string body, string kind)
    {
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Body = body,
            Kind = kind,
            CreatedUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync().ConfigureAwait(false);
    }
}
