using ApplicationService.Domain.Entities;

namespace ApplicationService.Application.Abstractions;

public interface IApplicationsRepository
{
    Task<bool> ExistsForJobAndCandidateAsync(Guid jobId, Guid candidateId, CancellationToken ct);
    void Add(JobApplication application);
    Task<JobApplication?> GetTrackedAsync(Guid id, CancellationToken ct);
    void Remove(JobApplication application);
    Task<IReadOnlyList<JobApplication>> ListForCandidateAsync(Guid candidateId, CancellationToken ct);
    Task<IReadOnlyList<JobApplication>> ListForRecruiterAsync(Guid recruiterId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
