namespace MessagingService.Domain.Entities;

public sealed class Conversation
{
    public Guid Id { get; set; }
    public Guid ParticipantLowId { get; set; }
    public Guid ParticipantHighId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
