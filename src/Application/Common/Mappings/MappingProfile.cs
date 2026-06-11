using Domain.Entities;
using Application.Companies.DTOs;
using AutoMapper;

namespace Application.Common.Mappings;

public class MappingProfile : Profile
{
  public MappingProfile()
  {
    // Entity -> DTO mapping
    CreateMap<Company, CompanyDto>();
  }
}
