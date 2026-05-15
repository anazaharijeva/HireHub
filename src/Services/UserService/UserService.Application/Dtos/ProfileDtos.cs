namespace UserService.Application.Dtos;

public sealed record CandidateProfileDto(
    Guid UserId,
    string FullName,
    string? ProfilePictureUrl,
    string? Skills,
    string? Experience,
    string? Education,
    string? CvUrl,
    string? LinkedInUrl,
    string? GitHubUrl,
    DateTime UpdatedUtc);

public sealed record UpsertCandidateProfileRequest(
    string FullName,
    string? ProfilePictureUrl,
    string? Skills,
    string? Experience,
    string? Education,
    string? CvUrl,
    string? LinkedInUrl,
    string? GitHubUrl);

public sealed record RecruiterProfileDto(
    Guid UserId,
    string CompanyName,
    string? Position,
    string? CompanyDescription,
    string? CompanyLogoUrl,
    DateTime UpdatedUtc);

public sealed record UpsertRecruiterProfileRequest(
    string CompanyName,
    string? Position,
    string? CompanyDescription,
    string? CompanyLogoUrl);
