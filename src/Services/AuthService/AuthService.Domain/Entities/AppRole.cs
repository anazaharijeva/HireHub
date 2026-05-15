namespace AuthService.Domain.Entities;

public sealed class AppRole
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
