using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Interviews.Commands;

public record RescheduleInterviewCommand(Guid JobApplicationId, Guid InterviewId, DateTime NewScheduledAt) : IRequest;

public class RescheduleInterviewHandler : IRequestHandler<RescheduleInterviewCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RescheduleInterviewHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(RescheduleInterviewCommand request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        var interview = await _context.Interviews
            .FirstOrDefaultAsync(i => i.Id == request.InterviewId && i.JobApplicationId == request.JobApplicationId, ct)
            ?? throw new KeyNotFoundException($"Interview {request.InterviewId} not found");

        interview.Reschedule(request.NewScheduledAt);
        await _context.SaveChangesAsync(ct);
    }
}
