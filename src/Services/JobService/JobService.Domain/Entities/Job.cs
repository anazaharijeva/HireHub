using JobService.Domain;

namespace JobService.Domain.Entities;

public sealed class Job
{
    public Guid Id { get; set; }
    public Guid CreatorUserId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Requirements { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public required string Location { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public required string CompanyName { get; set; }
    public int CategoryId { get; set; }
    public JobCategory Category { get; set; } = null!;
    public DateTime PostedUtc { get; set; }
}
