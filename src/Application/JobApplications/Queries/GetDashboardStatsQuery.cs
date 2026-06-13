using Application.Common.Interfaces;
using Application.JobApplications.DTOs;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.JobApplications.Queries;

public record GetDashboardStatsQuery() : IRequest<DashboardDto>;

public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, DashboardDto>
{
  private readonly IApplicationDbContext _context;
  private readonly ICurrentUserService _currentUser;

  public GetDashboardStatsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
  {
    _context = context;
    _currentUser = currentUser;
  }

  public async Task<DashboardDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
  {
    var userId = _currentUser.UserId;

    var applications = await _context.JobApplications
      .Where(a => a.UserId == userId)
      .ToListAsync(ct);

    var total = applications.Count;

    var activeStatuses = new[] {
      ApplicationStatus.Saved, ApplicationStatus.Applied,
      ApplicationStatus.PhoneScreen, ApplicationStatus.TechnicalInterview,
      ApplicationStatus.OnSiteInterview, ApplicationStatus.Offer
    };

    var active = applications.Count(a => activeStatuses.Contains(a.Status));
    var withInterview = applications.Count(a =>
        a.Status >= ApplicationStatus.PhoneScreen
        && a.Status != ApplicationStatus.Rejected
        && a.Status != ApplicationStatus.Withdrawn);
    var offers = applications.Count(a => a.Status == ApplicationStatus.Offer);
    var rejected = applications.Count(a => a.Status == ApplicationStatus.Rejected);

    var statusBreakdown = applications
      .GroupBy(a => a.Status)
      .ToDictionary(g => g.Key.ToString(), g => g.Count());

    return new DashboardDto
    {
      TotalApplications = total,
      ActiveApplications = active,
      InterviewsScheduled = withInterview,
      Offers = offers,
      Rejected = rejected,
      ResponseRate = total > 0
        ? Math.Round((double)withInterview / total * 100, 1)
        : null,
      InterviewToOfferRate = withInterview > 0
        ? Math.Round((double)offers / withInterview * 100, 1)
        : null,
      StatusBreakdown = statusBreakdown
    };
  }
}
