using UserService.Application.Abstractions;
using UserService.Application.Dtos;
using UserService.Domain.Entities;

namespace UserService.Application.Services;

public interface IUserProfileService
{
    Task<CandidateProfileDto?> GetCandidateAsync(Guid userId, CancellationToken ct);
    Task<RecruiterProfileDto?> GetRecruiterAsync(Guid userId, CancellationToken ct);
    Task UpsertCandidateAsync(Guid userId, UpsertCandidateProfileRequest request, CancellationToken ct);
    Task UpsertRecruiterAsync(Guid userId, UpsertRecruiterProfileRequest request, CancellationToken ct);
    Task<IReadOnlyList<CandidateProfileDto>> SearchCandidatesAsync(string? q, CancellationToken ct);
}

public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserDb _db;

    public UserProfileService(IUserDb db) => _db = db;

    public async Task<CandidateProfileDto?> GetCandidateAsync(Guid userId, CancellationToken ct)
    {
        var e = await _db.GetCandidateAsync(userId, ct).ConfigureAwait(false);
        return e is null ? null : Map(e);
    }

    public async Task<RecruiterProfileDto?> GetRecruiterAsync(Guid userId, CancellationToken ct)
    {
        var e = await _db.GetRecruiterAsync(userId, ct).ConfigureAwait(false);
        return e is null ? null : Map(e);
    }

    public async Task UpsertCandidateAsync(Guid userId, UpsertCandidateProfileRequest r, CancellationToken ct)
    {
        var e = await _db.GetCandidateAsync(userId, ct).ConfigureAwait(false) ?? new CandidateProfile { UserId = userId, FullName = "" };
        e.FullName = r.FullName;
        e.ProfilePictureUrl = r.ProfilePictureUrl;
        e.Skills = r.Skills;
        e.Experience = r.Experience;
        e.Education = r.Education;
        e.CvUrl = r.CvUrl;
        e.LinkedInUrl = r.LinkedInUrl;
        e.GitHubUrl = r.GitHubUrl;
        e.UpdatedUtc = DateTime.UtcNow;
        _db.UpsertCandidate(e);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task UpsertRecruiterAsync(Guid userId, UpsertRecruiterProfileRequest r, CancellationToken ct)
    {
        var e = await _db.GetRecruiterAsync(userId, ct).ConfigureAwait(false) ?? new RecruiterProfile { UserId = userId, CompanyName = "" };
        e.CompanyName = r.CompanyName;
        e.Position = r.Position;
        e.CompanyDescription = r.CompanyDescription;
        e.CompanyLogoUrl = r.CompanyLogoUrl;
        e.UpdatedUtc = DateTime.UtcNow;
        _db.UpsertRecruiter(e);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CandidateProfileDto>> SearchCandidatesAsync(string? q, CancellationToken ct)
    {
        var list = await _db.SearchCandidatesAsync(q, 50, ct).ConfigureAwait(false);
        return list.Select(Map).ToList();
    }

    private static CandidateProfileDto Map(CandidateProfile c) =>
        new(c.UserId, c.FullName, c.ProfilePictureUrl, c.Skills, c.Experience, c.Education, c.CvUrl, c.LinkedInUrl, c.GitHubUrl, c.UpdatedUtc);

    private static RecruiterProfileDto Map(RecruiterProfile r) =>
        new(r.UserId, r.CompanyName, r.Position, r.CompanyDescription, r.CompanyLogoUrl, r.UpdatedUtc);
}
