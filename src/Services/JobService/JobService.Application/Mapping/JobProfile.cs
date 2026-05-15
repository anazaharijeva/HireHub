using AutoMapper;
using JobService.Application.Dtos;
using JobService.Domain.Entities;

namespace JobService.Application.Mapping;

public sealed class JobProfile : Profile
{
    public JobProfile()
    {
        CreateMap<Job, JobDto>()
            .ForMember(d => d.EmploymentType, o => o.MapFrom(s => s.EmploymentType.ToString()))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name));
    }
}
