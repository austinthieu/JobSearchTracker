using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.JobApplications.Commands;

public record CreateJobApplicationCommand(string Position, Guid CompanyId, string? JobUrl, string? Source, decimal? SalaryMin, decimal? SalaryMax) : IRequest<Guid>;

public class CreateJobApplicationHandler : IRequestHandler<CreateJobApplicationCommand, Guid>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public CreateJobApplicationHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task<Guid> Handle(CreateJobApplicationCommand request, CancellationToken ct)
  {
    var jobApplication = new JobApplication(request.Position, request.CompanyId,
        request.JobUrl, request.Source, request.SalaryMin, request.SalaryMax, _currentUser.UserId);
    _context.JobApplications.Add(jobApplication);
    await _context.SaveChangesAsync(ct);
    return jobApplication.Id;
  }
}
