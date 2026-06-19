using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Interviews.Commands;

public record CreateInterviewCommand(
    Guid JobApplicationId,
    string Type,
    DateTime ScheduledAt,
    int? DurationMinutes,
    string? Location,
    string? Interviewers,
    string? Notes
) : IRequest<Guid>;

public class CreateInterviewHandler : IRequestHandler<CreateInterviewCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateInterviewHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateInterviewCommand request, CancellationToken ct)
    {
        var jobApp = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == request.JobApplicationId && j.UserId == _currentUser.UserId, ct)
            ?? throw new KeyNotFoundException($"Job application {request.JobApplicationId} not found");

        if (!Enum.TryParse<InterviewType>(request.Type, true, out var type))
            throw new InvalidOperationException($"Invalid interview type '{request.Type}'. Valid values: {string.Join(", ", Enum.GetNames<InterviewType>())}");

        var interview = new Interview(
            request.JobApplicationId, type, request.ScheduledAt,
            request.DurationMinutes, request.Location,
            request.Interviewers, request.Notes
        );

        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync(ct);
        return interview.Id;
    }
}
