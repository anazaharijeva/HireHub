using JobService.Application.Dtos;
using JobService.Domain.Entities;

namespace JobService.Application.Abstractions;

public interface IJobDb
{
    Task<(IReadOnlyList<Job> Items, int Total)> QueryJobsAsync(JobFilter filter, CancellationToken ct);
    Task<Job?> GetJobAsync(Guid id, CancellationToken ct);
    Task<Job?> GetJobForEditAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<JobCategory>> GetCategoriesAsync(CancellationToken ct);
    void AddJob(Job job);
    void RemoveJob(Job job);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
