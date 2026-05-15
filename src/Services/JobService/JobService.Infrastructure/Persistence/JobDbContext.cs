using JobService.Application.Abstractions;
using JobService.Application.Dtos;
using JobService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobService.Infrastructure.Persistence;

public sealed class JobDbContext : DbContext, IJobDb
{
    public JobDbContext(DbContextOptions<JobDbContext> options) : base(options) { }

    public Task<Job?> GetJobAsync(Guid id, CancellationToken ct) =>
        Set<Job>().AsNoTracking().Include(j => j.Category).FirstOrDefaultAsync(j => j.Id == id, ct);

    public Task<Job?> GetJobForEditAsync(Guid id, CancellationToken ct) =>
        Set<Job>().Include(j => j.Category).FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<IReadOnlyList<JobCategory>> GetCategoriesAsync(CancellationToken ct) =>
        await Set<JobCategory>().AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct).ConfigureAwait(false);

    public async Task<(IReadOnlyList<Job> Items, int Total)> QueryJobsAsync(JobFilter filter, CancellationToken ct)
    {
        var q = Set<Job>().AsNoTracking().Include(j => j.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var loc = filter.Location.Trim().ToLowerInvariant();
            q = q.Where(j => j.Location.ToLower().Contains(loc));
        }

        if (filter.MinSalary is not null)
            q = q.Where(j => j.SalaryMax == null || j.SalaryMax >= filter.MinSalary);
        if (filter.MaxSalary is not null)
            q = q.Where(j => j.SalaryMin == null || j.SalaryMin <= filter.MaxSalary);
        if (filter.Type is not null)
            q = q.Where(j => j.EmploymentType == filter.Type);
        if (filter.CategoryId is not null)
            q = q.Where(j => j.CategoryId == filter.CategoryId);

        var total = await q.CountAsync(ct).ConfigureAwait(false);

        q = (filter.Sort?.ToLowerInvariant()) switch
        {
            "salary" => q.OrderByDescending(j => j.SalaryMax ?? j.SalaryMin ?? 0),
            "title" => q.OrderBy(j => j.Title),
            _ => q.OrderByDescending(j => j.PostedUtc)
        };

        var page = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync(ct).ConfigureAwait(false);
        return (items, total);
    }

    public void AddJob(Job job) => Set<Job>().Add(job);
    public void RemoveJob(Job job) => Set<Job>().Remove(job);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<Job>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.CompanyName).HasMaxLength(200);
            e.Property(x => x.Location).HasMaxLength(200);
            e.HasOne(x => x.Category).WithMany(c => c.Jobs).HasForeignKey(x => x.CategoryId);
        });
    }
}
