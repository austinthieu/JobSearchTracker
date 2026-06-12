using Application.Common.Interfaces;
using Application.JobApplications.DTOs;
using MediatR;

namespace Application.JobApplications.Queries;

public record GetJobApplicationByIdQuery(Guid Id) : IRequest<ApplicationDto?>;

public class GetJobApplicationByIdHandler : IRequestHandler<GetJobApplicationByIdQuery, ApplicationDto?>
{
  private readonly IApplicationDbContext _context;

  public GetJobApplicationByIdHandler(IApplicationDbContext context) => _context = context;

  public async Task<ApplicationDto?> Handle(GetJobApplicationByIdQuery request, CancellationToken ct)
  {
    var jobApplication = await _context.JobApplications.FindAsync([request.Id], ct);
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
