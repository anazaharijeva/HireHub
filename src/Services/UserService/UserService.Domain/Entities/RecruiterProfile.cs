namespace UserService.Domain.Entities;

public sealed class RecruiterProfile
{
    public Guid UserId { get; set; }
    public required string CompanyName { get; set; }
    public string? Position { get; set; }
    public string? CompanyDescription { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
