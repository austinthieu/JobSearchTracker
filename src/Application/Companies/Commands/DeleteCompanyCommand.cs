using Application.Common.Interfaces;
using MediatR;

namespace Application.Companies.Commands;

public record DeleteCompanyCommand(Guid Id) : IRequest;

public class DeleteCompanyHandler : IRequestHandler<DeleteCompanyCommand>
{
  private readonly IApplicationDbContext _context;

  public DeleteCompanyHandler(IApplicationDbContext context) => _context = context;

  public async Task Handle(DeleteCompanyCommand request, CancellationToken ct)
  {
    var company = await _context.Companies.FindAsync([request.Id], ct)
        ?? throw new KeyNotFoundException($"Company {request.Id} not found");

    _context.Companies.Remove(company);
    await _context.SaveChangesAsync(ct);
  }
}
