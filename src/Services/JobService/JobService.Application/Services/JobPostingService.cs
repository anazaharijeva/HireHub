using AutoMapper;
using HireHub.Contracts.Events;
using HireHub.Contracts.Messaging;
using JobService.Application.Abstractions;
using JobService.Application.Dtos;
using JobService.Domain;
using JobService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace JobService.Application.Services;

public interface IJobPostingService
{
    Task<(IReadOnlyList<JobDto> Items, int Total)> ListAsync(JobFilter filter, CancellationToken ct);
    Task<JobDto?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<(int Id, string Name)>> CategoriesAsync(CancellationToken ct);
    Task<JobDto> CreateAsync(Guid creatorUserId, CreateJobRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, Guid userId, CreateJobRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct);
}

public sealed class JobPostingService : IJobPostingService
{
    private readonly IJobDb _db;
    private readonly IMapper _mapper;
    private readonly IIntegrationEventPublisher _events;
    private readonly ILogger<JobPostingService> _logger;

    public JobPostingService(IJobDb db, IMapper mapper, IIntegrationEventPublisher events, ILogger<JobPostingService> logger)
    {
        _db = db;
        _mapper = mapper;
        _events = events;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<JobDto> Items, int Total)> ListAsync(JobFilter filter, CancellationToken ct)
    {
        var (items, total) = await _db.QueryJobsAsync(filter, ct).ConfigureAwait(false);
        return (_mapper.Map<IReadOnlyList<JobDto>>(items), total);
    }

    public async Task<JobDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var j = await _db.GetJobAsync(id, ct).ConfigureAwait(false);
        return j is null ? null : _mapper.Map<JobDto>(j);
    }

    public async Task<IReadOnlyList<(int Id, string Name)>> CategoriesAsync(CancellationToken ct)
    {
        var c = await _db.GetCategoriesAsync(ct).ConfigureAwait(false);
        return c.Select(x => (x.Id, x.Name)).ToList();
    }

    public async Task<JobDto> CreateAsync(Guid creatorUserId, CreateJobRequest request, CancellationToken ct)
    {
        var categories = await _db.GetCategoriesAsync(ct).ConfigureAwait(false);
        if (categories.All(c => c.Id != request.CategoryId))
            throw new InvalidOperationException("Unknown category.");

        var job = new Job
        {
            Id = Guid.NewGuid(),
            CreatorUserId = creatorUserId,
            Title = request.Title,
            Description = request.Description,
            Requirements = request.Requirements,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Location = request.Location,
            EmploymentType = request.EmploymentType,
            CompanyName = request.CompanyName,
            CategoryId = request.CategoryId,
            PostedUtc = DateTime.UtcNow
        };

        _db.AddJob(job);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);

        var loaded = await _db.GetJobAsync(job.Id, ct).ConfigureAwait(false) ?? job;
        var dto = _mapper.Map<JobDto>(loaded);
        try
        {
            await _events.PublishAsync(new JobCreatedEvent(job.Id, creatorUserId, job.Title, job.CompanyName, DateTime.UtcNow), ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish JobCreatedEvent");
        }

        return dto;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, CreateJobRequest request, CancellationToken ct)
    {
        var job = await _db.GetJobForEditAsync(id, ct).ConfigureAwait(false);
        if (job is null || job.CreatorUserId != userId)
            return false;

        var categories = await _db.GetCategoriesAsync(ct).ConfigureAwait(false);
        var cat = categories.FirstOrDefault(c => c.Id == request.CategoryId);
        if (cat is null)
            return false;

        job.Title = request.Title;
        job.Description = request.Description;
        job.Requirements = request.Requirements;
        job.SalaryMin = request.SalaryMin;
        job.SalaryMax = request.SalaryMax;
        job.Location = request.Location;
        job.EmploymentType = request.EmploymentType;
        job.CompanyName = request.CompanyName;
        job.CategoryId = cat.Id;
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var job = await _db.GetJobForEditAsync(id, ct).ConfigureAwait(false);
        if (job is null || job.CreatorUserId != userId)
            return false;
        _db.RemoveJob(job);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }
}
