using Application.Common.Interfaces;
using Application.JobApplications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.JobApplications.Queries;

public record GetJobApplicationsQuery : IRequest<List<ApplicationDto>>;

public class GetJobApplicationsHandler : IRequestHandler<GetJobApplicationsQuery, List<ApplicationDto>>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public GetJobApplicationsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task<List<ApplicationDto>> Handle(GetJobApplicationsQuery request, CancellationToken ct)
  {
    return await _context.JobApplications
      .Where(c => c.UserId == _currentUser.UserId)
      .OrderBy(c => c.Id)
      .Select(c => new ApplicationDto
      {
        Id = c.Id,
        Position = c.Position,
        SalaryMin = c.SalaryMin,
        SalaryMax = c.SalaryMax,
        JobUrl = c.JobUrl,
        Source = c.Source,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
      })
      .ToListAsync(ct);
  }
}
