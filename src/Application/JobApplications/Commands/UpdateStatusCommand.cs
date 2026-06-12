using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.JobApplications.Commands;

public record UpdateStatusCommand(Guid Id, ApplicationStatus Status) : IRequest;

public class UpdateStatusHandler : IRequestHandler<UpdateStatusCommand>
{
  private readonly IApplicationDbContext _context;

  public UpdateStatusHandler(IApplicationDbContext context) => _context = context;

  public async Task Handle(UpdateStatusCommand request, CancellationToken ct)
  {
    var jobApplication = await _context.JobApplications.FindAsync([request.Id], ct)
        ?? throw new KeyNotFoundException($"Job Application {request.Id} not found");

    var statusHistory = new ApplicationStatusHistory(jobApplication.Id, jobApplication.Status, request.Status);
    _context.ApplicationStatusHistories.Add(statusHistory);

    jobApplication.UpdateStatus(request.Status);
    await _context.SaveChangesAsync(ct);
  }
}
