namespace JobService.Domain.Entities;

public sealed class JobCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
