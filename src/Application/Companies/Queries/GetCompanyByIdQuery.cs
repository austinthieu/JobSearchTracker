using Application.Common.Interfaces;
using Application.Companies.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Companies.Queries;

public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto?>;

public class GetCompanyByIdHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
  private readonly IApplicationDbContext _context;
  private readonly IMapper _mapper;

  public GetCompanyByIdHandler(IApplicationDbContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken ct)
  {
    var company = await _context.Companies.FindAsync([request.Id], ct);
    return company is null ? null : _mapper.Map<CompanyDto>(company);
  }
}
