using Application.Common.Interfaces;
using Application.Companies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Queries;

public record GetCompaniesQuery : IRequest<List<CompanyDto>>;

public class GetCompaniesHandler : IRequestHandler<GetCompaniesQuery, List<CompanyDto>>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public GetCompaniesHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task<List<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken ct)
  {
    return await _context.Companies
      .Where(c => c.UserId == _currentUser.UserId)
      .OrderBy(c => c.Name)
      .Select(c => new CompanyDto
      {
        Id = c.Id,
        Name = c.Name,
        Website = c.Website,
        Notes = c.Notes,
        CreatedAt = c.CreatedAt
      })
      .ToListAsync(ct);
  }
}
