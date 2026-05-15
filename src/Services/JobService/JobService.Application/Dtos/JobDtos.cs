using JobService.Domain;

namespace JobService.Application.Dtos;

public sealed record JobDto(
    Guid Id,
    Guid CreatorUserId,
    string Title,
    string Description,
    string? Requirements,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string Location,
    string EmploymentType,
    string CompanyName,
    int CategoryId,
    string CategoryName,
    DateTime PostedUtc);

public sealed record JobFilter(
    string? Location,
    decimal? MinSalary,
    decimal? MaxSalary,
    EmploymentType? Type,
    int? CategoryId,
    int Page,
    int PageSize,
    string Sort);

public sealed record CreateJobRequest(
    string Title,
    string Description,
    string? Requirements,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string Location,
    EmploymentType EmploymentType,
    string CompanyName,
    int CategoryId);
