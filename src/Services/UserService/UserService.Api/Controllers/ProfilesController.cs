using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Dtos;
using UserService.Application.Services;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public sealed class ProfilesController : ControllerBase
{
    private readonly IUserProfileService _profiles;

    public ProfilesController(IUserProfileService profiles) => _profiles = profiles;

    private Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing sub claim"));

    [HttpGet("candidate/{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CandidateProfileDto>> GetCandidate(Guid userId, CancellationToken ct)
    {
        var p = await _profiles.GetCandidateAsync(userId, ct).ConfigureAwait(false);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpGet("candidate/me")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<CandidateProfileDto>> GetMyCandidate(CancellationToken ct)
    {
        var p = await _profiles.GetCandidateAsync(UserId, ct).ConfigureAwait(false);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPut("candidate/me")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> UpsertMyCandidate([FromBody] UpsertCandidateProfileRequest request, CancellationToken ct)
    {
        await _profiles.UpsertCandidateAsync(UserId, request, ct).ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("recruiter/{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<RecruiterProfileDto>> GetRecruiter(Guid userId, CancellationToken ct)
    {
        var p = await _profiles.GetRecruiterAsync(userId, ct).ConfigureAwait(false);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpGet("recruiter/me")]
    [Authorize(Roles = "Recruiter")]
    public async Task<ActionResult<RecruiterProfileDto>> GetMyRecruiter(CancellationToken ct)
    {
        var p = await _profiles.GetRecruiterAsync(UserId, ct).ConfigureAwait(false);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPut("recruiter/me")]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> UpsertMyRecruiter([FromBody] UpsertRecruiterProfileRequest request, CancellationToken ct)
    {
        await _profiles.UpsertRecruiterAsync(UserId, request, ct).ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("candidates/search")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CandidateProfileDto>>> Search([FromQuery] string? q, CancellationToken ct) =>
        Ok(await _profiles.SearchCandidatesAsync(q, ct).ConfigureAwait(false));
}
