using Application.Common.Interfaces;
using Application.JobApplications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.JobApplications.Queries;

public record GetJobApplicationByIdQuery(Guid Id) : IRequest<ApplicationDto?>;

public class GetJobApplicationByIdHandler : IRequestHandler<GetJobApplicationByIdQuery, ApplicationDto?>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public GetJobApplicationByIdHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task<ApplicationDto?> Handle(GetJobApplicationByIdQuery request, CancellationToken ct)
  {
    var jobApplication = await _context.JobApplications.
      FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == _currentUser.UserId, ct);
    return jobApplication is null ? null : new ApplicationDto
    {
      Id = jobApplication.Id,
      Position = jobApplication.Position,
      SalaryMin = jobApplication.SalaryMin,
      SalaryMax = jobApplication.SalaryMax,
      JobUrl = jobApplication.JobUrl,
      Source = jobApplication.Source,
      CreatedAt = jobApplication.CreatedAt,
      UpdatedAt = jobApplication.UpdatedAt,
    };
  }
}
