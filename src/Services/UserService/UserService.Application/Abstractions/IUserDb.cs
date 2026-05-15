using UserService.Domain.Entities;

namespace UserService.Application.Abstractions;

public interface IUserDb
{
    Task<CandidateProfile?> GetCandidateAsync(Guid userId, CancellationToken ct);
    Task<RecruiterProfile?> GetRecruiterAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<CandidateProfile>> SearchCandidatesAsync(string? q, int take, CancellationToken ct);
    void UpsertCandidate(CandidateProfile profile);
    void UpsertRecruiter(RecruiterProfile profile);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
