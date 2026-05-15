using System.Security.Claims;
using JobService.Application.Dtos;
using JobService.Application.Services;
using JobService.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobService.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public sealed class JobsController : ControllerBase
{
    private readonly IJobPostingService _jobs;

    public JobsController(IJobPostingService jobs) => _jobs = jobs;

    private Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Missing sub claim"));

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] string? location,
        [FromQuery] decimal? minSalary,
        [FromQuery] decimal? maxSalary,
        [FromQuery] EmploymentType? type,
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sort = "posted",
        CancellationToken ct = default)
    {
        var filter = new JobFilter(location, minSalary, maxSalary, type, categoryId, page, pageSize, sort);
        var (items, total) = await _jobs.ListAsync(filter, ct).ConfigureAwait(false);
        return Ok(new { total, items });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobDto>> Get(Guid id, CancellationToken ct)
    {
        var j = await _jobs.GetAsync(id, ct).ConfigureAwait(false);
        return j is null ? NotFound() : Ok(j);
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> Categories(CancellationToken ct) =>
        Ok(await _jobs.CategoriesAsync(ct).ConfigureAwait(false));

    [HttpPost]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<JobDto>> Create([FromBody] CreateJobRequest request, CancellationToken ct) =>
        Ok(await _jobs.CreateAsync(UserId, request, ct).ConfigureAwait(false));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateJobRequest request, CancellationToken ct) =>
        await _jobs.UpdateAsync(id, UserId, request, ct).ConfigureAwait(false) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await _jobs.DeleteAsync(id, UserId, ct).ConfigureAwait(false) ? NoContent() : NotFound();
}
