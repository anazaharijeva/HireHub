using System.Security.Claims;
using ApplicationService.Application.Abstractions;
using ApplicationService.Application.Applications;
using ApplicationService.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationService.Api.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationsRepository _repo;

    public ApplicationsController(IMediator mediator, IApplicationsRepository repo)
    {
        _mediator = mediator;
        _repo = repo;
    }

    private Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing sub claim"));

    public sealed record ApplyRequest(Guid JobId, Guid? RecruiterUserId, string? CoverLetter);

    public sealed record StatusRequest(ApplicationStatus Status);

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Apply([FromBody] ApplyRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApplyToJobCommand(UserId, request.JobId, request.RecruiterUserId, request.CoverLetter), ct).ConfigureAwait(false);
        if (!result.Ok)
            return Conflict(new { error = result.Error });
        return Created($"/api/applications/{result.ApplicationId}", new { id = result.ApplicationId });
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> MyApplications(CancellationToken ct) =>
        Ok(await _repo.ListForCandidateAsync(UserId, ct).ConfigureAwait(false));

    [HttpGet("recruiter/mine")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> RecruiterApplications(CancellationToken ct) =>
        Ok(await _repo.ListForRecruiterAsync(UserId, ct).ConfigureAwait(false));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] StatusRequest request, CancellationToken ct) =>
        await _mediator.Send(new UpdateApplicationStatusCommand(id, UserId, request.Status), ct).ConfigureAwait(false)
            ? NoContent()
            : NotFound();

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> Withdraw(Guid id, CancellationToken ct) =>
        await _mediator.Send(new WithdrawApplicationCommand(id, UserId), ct).ConfigureAwait(false)
            ? NoContent()
            : NotFound();
}
