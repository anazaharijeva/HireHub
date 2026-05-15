using HireHub.Contracts.Events;
using HireHub.Contracts.Messaging;
using MessagingService.Domain.Entities;
using MessagingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Infrastructure;

public interface IMessagingService
{
    Task<IReadOnlyList<Conversation>> ListConversationsAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Message>> GetMessagesAsync(Guid conversationId, Guid userId, CancellationToken ct);
    Task<Message> SendAsync(Guid fromUserId, Guid toUserId, string body, CancellationToken ct);
    Task MarkReadAsync(Guid conversationId, Guid userId, CancellationToken ct);
}

public sealed class MessagingAppService : IMessagingService
{
    private readonly MessagingDbContext _db;
    private readonly IIntegrationEventPublisher _events;

    public MessagingAppService(MessagingDbContext db, IIntegrationEventPublisher events)
    {
        _db = db;
        _events = events;
    }

    public async Task<IReadOnlyList<Conversation>> ListConversationsAsync(Guid userId, CancellationToken ct) =>
        await _db.Conversations.AsNoTracking()
            .Where(c => c.ParticipantLowId == userId || c.ParticipantHighId == userId)
            .OrderByDescending(c => c.CreatedUtc)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Message>> GetMessagesAsync(Guid conversationId, Guid userId, CancellationToken ct)
    {
        var conv = await _db.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId, ct).ConfigureAwait(false);
        if (conv is null || (conv.ParticipantLowId != userId && conv.ParticipantHighId != userId))
            return Array.Empty<Message>();

        return await _db.Messages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentUtc)
            .ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<Message> SendAsync(Guid fromUserId, Guid toUserId, string body, CancellationToken ct)
    {
        var low = fromUserId < toUserId ? fromUserId : toUserId;
        var high = fromUserId < toUserId ? toUserId : fromUserId;

        var conv = await _db.Conversations
            .FirstOrDefaultAsync(c => c.ParticipantLowId == low && c.ParticipantHighId == high, ct).ConfigureAwait(false);

        if (conv is null)
        {
            conv = new Conversation
            {
                Id = Guid.NewGuid(),
                ParticipantLowId = low,
                ParticipantHighId = high,
                CreatedUtc = DateTime.UtcNow
            };
            _db.Conversations.Add(conv);
        }

        var msg = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conv.Id,
            SenderId = fromUserId,
            RecipientId = toUserId,
            Body = body,
            SentUtc = DateTime.UtcNow
        };
        _db.Messages.Add(msg);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        var preview = body.Length > 120 ? body[..120] : body;
        try
        {
            await _events.PublishAsync(new MessageSentEvent(conv.Id, fromUserId, toUserId, msg.Id, preview, DateTime.UtcNow), ct).ConfigureAwait(false);
        }
        catch
        {
            // optional: log
        }

        return msg;
    }

    public async Task MarkReadAsync(Guid conversationId, Guid userId, CancellationToken ct)
    {
        var msgs = await _db.Messages.Where(m => m.ConversationId == conversationId && m.RecipientId == userId && m.ReadUtc == null)
            .ToListAsync(ct).ConfigureAwait(false);
        foreach (var m in msgs)
            m.ReadUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
