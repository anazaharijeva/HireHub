using Microsoft.EntityFrameworkCore;
using UserService.Application.Abstractions;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence;

public sealed class UserDbContext : DbContext, IUserDb
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public Task<CandidateProfile?> GetCandidateAsync(Guid userId, CancellationToken ct) =>
        Set<CandidateProfile>().AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public Task<RecruiterProfile?> GetRecruiterAsync(Guid userId, CancellationToken ct) =>
        Set<RecruiterProfile>().AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task<IReadOnlyList<CandidateProfile>> SearchCandidatesAsync(string? q, int take, CancellationToken ct)
    {
        var query = Set<CandidateProfile>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var t = q.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(t) ||
                (c.Skills != null && c.Skills.ToLower().Contains(t)));
        }

        return await query.OrderByDescending(c => c.UpdatedUtc).Take(take).ToListAsync(ct).ConfigureAwait(false);
    }

    public void UpsertCandidate(CandidateProfile profile)
    {
        var exists = Set<CandidateProfile>().Any(e => e.UserId == profile.UserId);
        if (exists)
            Set<CandidateProfile>().Update(profile);
        else
            Set<CandidateProfile>().Add(profile);
    }

    public void UpsertRecruiter(RecruiterProfile profile)
    {
        var exists = Set<RecruiterProfile>().Any(e => e.UserId == profile.UserId);
        if (exists)
            Set<RecruiterProfile>().Update(profile);
        else
            Set<RecruiterProfile>().Add(profile);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CandidateProfile>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.FullName).HasMaxLength(200);
        });

        modelBuilder.Entity<RecruiterProfile>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.CompanyName).HasMaxLength(200);
        });
    }
}
