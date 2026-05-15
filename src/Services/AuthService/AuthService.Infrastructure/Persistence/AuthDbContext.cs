using AuthService.Application.Abstractions;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public sealed class AuthDbContext : DbContext, IAuthDb
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public IQueryable<AppUser> Users => Set<AppUser>();
    public IQueryable<AppRole> Roles => Set<AppRole>();
    public IQueryable<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public Task<AppUser?> FindUserByEmailAsync(string email, CancellationToken ct) =>
        Set<AppUser>()
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<AppUser?> FindUserWithRolesAsync(Guid id, CancellationToken ct) =>
        Set<AppUser>()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<RefreshToken?> FindRefreshTokenAsync(string token, CancellationToken ct) =>
        Set<RefreshToken>().FirstOrDefaultAsync(t => t.Token == token, ct);

    public Task<AppRole?> FindRoleByNameAsync(string name, CancellationToken ct) =>
        Set<AppRole>().FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower(), ct);

    public void AddUser(AppUser user) => Set<AppUser>().Add(user);
    public void AddRefreshToken(RefreshToken token) => Set<RefreshToken>().Add(token);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(320);
            e.Property(x => x.PasswordHash).HasMaxLength(200);
        });

        modelBuilder.Entity<AppRole>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(64);
        });

        modelBuilder.Entity<AppUserRole>(e =>
        {
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.Property(x => x.Token).HasMaxLength(200);
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId);
        });
    }
}
