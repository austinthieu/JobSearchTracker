using Application.Common.Interfaces;
using Application.Common.Models;
using Application.JobApplications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.JobApplications.Queries;

public record GetJobApplicationsQuery(int Page = 1, int PageSize = 25)
    : IRequest<PaginatedList<ApplicationDto>>;

public class GetJobApplicationsHandler
    : IRequestHandler<GetJobApplicationsQuery, PaginatedList<ApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetJobApplicationsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<ApplicationDto>> Handle(
        GetJobApplicationsQuery request,
        CancellationToken ct
    )
    {
        var query = _context
            .JobApplications.Where(c => c.UserId == _currentUser.UserId)
            .OrderBy(c => c.Id);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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

        return new PaginatedList<ApplicationDto>(items, totalCount, request.Page, request.PageSize);
    }
}
