using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.JobApplications.Commands;

public record CreateJobApplicationCommand(string Position, Guid CompanyId, string? JobUrl, string? Source, decimal? SalaryMin, decimal? SalaryMax, Guid UserId) : IRequest<Guid>;

public class CreateJobApplicationHandler : IRequestHandler<CreateJobApplicationCommand, Guid>
{
  private readonly IApplicationDbContext _context;

  public CreateJobApplicationHandler(IApplicationDbContext context) => _context = context;

  public async Task<Guid> Handle(CreateJobApplicationCommand request, CancellationToken ct)
  {
    var jobApplication = new JobApplication(request.Position, request.CompanyId, request.JobUrl, request.Source, request.SalaryMin, request.SalaryMax, request.UserId);
    _context.JobApplications.Add(jobApplication);
    await _context.SaveChangesAsync(ct);
    return jobApplication.Id;
  }
}
