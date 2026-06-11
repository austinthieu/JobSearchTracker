using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Companies.Commands;

public record CreateCompanyCommand(string Name, string? Website, string? Notes) : IRequest<Guid>;

public class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, Guid>
{
  private readonly IApplicationDbContext _context;

  public CreateCompanyHandler(IApplicationDbContext context) => _context = context;

  public async Task<Guid> Handle(CreateCompanyCommand request, CancellationToken ct)
  {
    var company = new Company(request.Name, request.Website, request.Notes);
    _context.Companies.Add(company);
    await _context.SaveChangesAsync(ct);
    return company.Id;
  }
}
