using Application.Common.Interfaces;
using Application.Companies.DTOs;
using MediatR;

namespace Application.Companies.Queries;

public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto?>;

public class GetCompanyByIdHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
  private readonly IApplicationDbContext _context;

  public GetCompanyByIdHandler(IApplicationDbContext context) => _context = context;

  public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken ct)
  {
    var company = await _context.Companies.FindAsync([request.Id], ct);
    return company is null ? null : new CompanyDto
    {
      Id = company.Id,
      Name = company.Name,
      Website = company.Website,
      Notes = company.Notes,
      CreatedAt = company.CreatedAt
    };
  }
}
