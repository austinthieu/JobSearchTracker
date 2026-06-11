using Application.Common.Interfaces;
using Application.Companies.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public record GetCompaniesQuery : IRequest<List<CompanyDto>>;

public class GetCompaniesHandler : IRequestHandler<GetCompaniesQuery, List<CompanyDto>>
{
  private readonly IApplicationDbContext _context;
  private readonly IMapper _mapper;

  public GetCompaniesHandler(IApplicationDbContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  public async Task<List<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken ct)
  {
    return await _context.Companies
      .OrderBy(c => c.Name)
      .ProjectTo<CompanyDto>(_mapper.ConfigurationProvider)
      .ToListAsync(ct);
  }
}


