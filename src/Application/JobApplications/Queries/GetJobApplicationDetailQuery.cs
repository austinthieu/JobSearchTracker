using Application.Common.Interfaces;
using Application.JobApplications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.JobApplications.Queries;

public record GetJobApplicationDetailQuery(Guid Id) : IRequest<ApplicationDetailDto?>;

public class GetJobApplicationDetailHandler : IRequestHandler<GetJobApplicationDetailQuery, ApplicationDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetJobApplicationDetailHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApplicationDetailDto?> Handle(GetJobApplicationDetailQuery request, CancellationToken ct)
    {
        var jobApplication = await _context.JobApplications
            .Include(j => j.Company)
            .Include(j => j.Interviews)
            .Include(j => j.Contacts)
            .Include(j => j.Notes)
            .Include(j => j.StatusHistory)
            .FirstOrDefaultAsync(j => j.Id == request.Id && j.UserId == _currentUser.UserId, ct);

        if (jobApplication is null)
            return null;

        return new ApplicationDetailDto
        {
            Id = jobApplication.Id,
            Position = jobApplication.Position,
            SalaryMin = jobApplication.SalaryMin?.ToString(),
            SalaryMax = jobApplication.SalaryMax?.ToString(),
            JobUrl = jobApplication.JobUrl,
            Source = jobApplication.Source,
            Status = jobApplication.Status.ToString(),
            CreatedAt = jobApplication.CreatedAt,
            UpdatedAt = jobApplication.UpdatedAt,
            CompanyId = jobApplication.CompanyId,
            CompanyName = jobApplication.Company.Name,
            CompanyWebsite = jobApplication.Company.Website,
            Interviews = jobApplication.Interviews
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
                .ToList(),
            Contacts = jobApplication.Contacts
                .OrderBy(c => c.Name)
                .Select(c => new ContactDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    Title = c.Title,
                    Notes = c.Notes,
                })
                .ToList(),
            Notes = jobApplication.Notes
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NoteDto
                {
                    Id = n.Id,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt,
                })
                .ToList(),
            StatusHistory = jobApplication.StatusHistory
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new StatusHistoryDto
                {
                    FromStatus = h.FromStatus.ToString(),
                    ToStatus = h.ToStatus.ToString(),
                    ChangedAt = h.ChangedAt,
                })
                .ToList(),
        };
    }
}
