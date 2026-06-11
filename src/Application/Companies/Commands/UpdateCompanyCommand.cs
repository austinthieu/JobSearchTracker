using Application.Common.Interfaces;
using MediatR;

namespace Application.Companies.Commands;

public record UpdateCompanyCommand(Guid Id, string Name, string? Website, string? Notes) : IRequest;

public class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand>
{
  private readonly IApplicationDbContext _context;

  public UpdateCompanyHandler(IApplicationDbContext context) => _context = context;

  public async Task Handle(UpdateCompanyCommand request, CancellationToken ct)
  {
    var company = await _context.Companies.FindAsync([request.Id], ct)
        ?? throw new KeyNotFoundException($"Company {request.Id} not found");

    company.Update(request.Name, request.Website, request.Notes);
    await _context.SaveChangesAsync(ct);
  }
}
