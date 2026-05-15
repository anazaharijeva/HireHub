namespace AuthService.Domain.Entities;

public sealed class AppUserRole
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public int RoleId { get; set; }
    public AppRole Role { get; set; } = null!;
}
