using AuthService.Domain.Entities;

namespace AuthService.Application.Abstractions;

public interface IAuthDb
{
    IQueryable<AppUser> Users { get; }
    IQueryable<AppRole> Roles { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }
    Task<AppUser?> FindUserByEmailAsync(string email, CancellationToken ct);
    Task<AppUser?> FindUserWithRolesAsync(Guid id, CancellationToken ct);
    Task<RefreshToken?> FindRefreshTokenAsync(string token, CancellationToken ct);
    Task<AppRole?> FindRoleByNameAsync(string name, CancellationToken ct);
    void AddUser(AppUser user);
    void AddRefreshToken(RefreshToken token);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
