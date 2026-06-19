using Application.Common.Interfaces;
using Application.JobApplications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Interviews.Queries;

public record GetInterviewsQuery(Guid JobApplicationId) : IRequest<List<InterviewDto>>;

public class GetInterviewsHandler : IRequestHandler<GetInterviewsQuery, List<InterviewDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetInterviewsHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<InterviewDto>> Handle(GetInterviewsQuery request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        return await _context.Interviews
            .Where(i => i.JobApplicationId == request.JobApplicationId)
            .OrderBy(i => i.ScheduledAt)
            .Select(i => new InterviewDto
            {
                Id = i.Id,
                Type = i.Type.ToString(),
                ScheduledAt = i.ScheduledAt,
                DurationMinutes = i.DurationMinutes,
                Location = i.Location,
                Interviewers = i.Interviewers,
                Notes = i.Notes,
                FollowUpSentAt = i.FollowUpSentAt,
            })
            .ToListAsync(ct);
    }
}
