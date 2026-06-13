using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Commands;

public record UpdateCompanyCommand(Guid Id, string Name, string? Website, string? Notes) : IRequest;

public class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public UpdateCompanyHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task Handle(UpdateCompanyCommand request, CancellationToken ct)
  {
    var company = await _context.Companies.
      FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == _currentUser.UserId, ct)
        ?? throw new KeyNotFoundException($"Company {request.Id} not found");

    company.Update(request.Name, request.Website, request.Notes);
    await _context.SaveChangesAsync(ct);
  }
}
