using Application.Common.Interfaces;
using Application.JobApplications.DTOs;
using MediatR;

namespace Application.JobApplications.Queries;

public record GetDashboardStats() : IRequest<DashboardDto?>;

public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStats, DashboardDto?>
{
  private readonly IApplicationDbContext _context;

  public GetDashboardStatsHandler(IApplicationDbContext context) => _context = context;

  public async Task<DashboardDto?> Handle(GetDashboardStats request, CancellationToken ct)
  {
    // TODO: Later after doing user stuff
    return null;
  }
}
