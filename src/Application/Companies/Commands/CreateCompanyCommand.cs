using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Companies.Commands;

public record CreateCompanyCommand(string Name, string? Website, string? Notes) : IRequest<Guid>;

public class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, Guid>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public CreateCompanyHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task<Guid> Handle(CreateCompanyCommand request, CancellationToken ct)
  {
    var company = new Company(request.Name, request.Website, request.Notes, _currentUser.UserId);
    _context.Companies.Add(company);
    await _context.SaveChangesAsync(ct);
    return company.Id;
  }
}
