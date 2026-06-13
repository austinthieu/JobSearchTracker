using Application.Common.Interfaces;
using Application.Companies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto?>;

public class GetCompanyByIdHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public GetCompanyByIdHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken ct)
  {
    var company = await _context.Companies.
      FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == _currentUser.UserId, ct);
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
