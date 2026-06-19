using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Interviews.Commands;

public record MarkFollowUpSentCommand(Guid JobApplicationId, Guid InterviewId) : IRequest;

public class MarkFollowUpSentHandler : IRequestHandler<MarkFollowUpSentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkFollowUpSentHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkFollowUpSentCommand request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        var interview = await _context.Interviews
            .FirstOrDefaultAsync(i => i.Id == request.InterviewId && i.JobApplicationId == request.JobApplicationId, ct)
            ?? throw new KeyNotFoundException($"Interview {request.InterviewId} not found");

        interview.MarkFollowUpSent();
        await _context.SaveChangesAsync(ct);
    }
}
