using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Companies.Commands;

public record DeleteCompanyCommand(Guid Id) : IRequest;

public class DeleteCompanyHandler : IRequestHandler<DeleteCompanyCommand>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public DeleteCompanyHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task Handle(DeleteCompanyCommand request, CancellationToken ct)
  {
    var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == _currentUser.UserId, ct)
        ?? throw new KeyNotFoundException($"Company {request.Id} not found");

    _context.Companies.Remove(company);
    await _context.SaveChangesAsync(ct);
  }
}
