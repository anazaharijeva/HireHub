namespace MessagingService.Domain.Entities;

public sealed class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public required string Body { get; set; }
    public DateTime SentUtc { get; set; }
    public DateTime? ReadUtc { get; set; }
}
