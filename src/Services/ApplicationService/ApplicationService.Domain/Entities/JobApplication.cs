using ApplicationService.Domain;

namespace ApplicationService.Domain.Entities;

public sealed class JobApplication
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid CandidateUserId { get; set; }
    public Guid? RecruiterUserId { get; set; }
    public ApplicationStatus Status { get; set; }
    public string? CoverLetter { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
