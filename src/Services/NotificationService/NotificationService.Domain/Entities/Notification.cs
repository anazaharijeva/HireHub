namespace NotificationService.Domain.Entities;

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string Kind { get; set; } = "system";
    public DateTime CreatedUtc { get; set; }
    public DateTime? ReadUtc { get; set; }
}
