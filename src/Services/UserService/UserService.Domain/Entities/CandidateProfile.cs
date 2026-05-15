namespace UserService.Domain.Entities;

public sealed class CandidateProfile
{
    public Guid UserId { get; set; }
    public required string FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Skills { get; set; }
    public string? Experience { get; set; }
    public string? Education { get; set; }
    public string? CvUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
