using ApplicationService.Application.Abstractions;
using ApplicationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApplicationService.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<JobApplication> Applications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobApplication>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.JobId, x.CandidateUserId }).IsUnique();
            e.Property(x => x.CoverLetter).HasMaxLength(4000);
        });
    }
}

public sealed class ApplicationsRepository : IApplicationsRepository
{
    private readonly ApplicationDbContext _db;

    public ApplicationsRepository(ApplicationDbContext db) => _db = db;

    public Task<bool> ExistsForJobAndCandidateAsync(Guid jobId, Guid candidateId, CancellationToken ct) =>
        _db.Applications.AnyAsync(a => a.JobId == jobId && a.CandidateUserId == candidateId, ct);

    public void Add(JobApplication application) => _db.Applications.Add(application);

    public void Remove(JobApplication application) => _db.Applications.Remove(application);

    public Task<JobApplication?> GetTrackedAsync(Guid id, CancellationToken ct) =>
        _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<JobApplication>> ListForCandidateAsync(Guid candidateId, CancellationToken ct) =>
        await _db.Applications.AsNoTracking()
            .Where(a => a.CandidateUserId == candidateId)
            .OrderByDescending(a => a.CreatedUtc)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<JobApplication>> ListForRecruiterAsync(Guid recruiterId, CancellationToken ct) =>
        await _db.Applications.AsNoTracking()
            .Where(a => a.RecruiterUserId == recruiterId)
            .OrderByDescending(a => a.CreatedUtc)
            .ToListAsync(ct).ConfigureAwait(false);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
