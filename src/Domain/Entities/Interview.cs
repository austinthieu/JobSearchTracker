using Domain.Enums;
using Domain.Common;

namespace Domain.Entities;

public class Interview : Entity
{
    public InterviewType Type { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public int? DurationMinutes { get; private set; }
    public string? Location { get; private set; } // URL, address, etc.
    public string? Interviewers { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? FollowUpSentAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Foreign Keys
    public Guid JobApplicationId { get; private set; }

    // Navigation
    public JobApplication JobApplication { get; private set; } = null!;

    private Interview() { }

    public Interview(Guid jobApplicationId, InterviewType type, DateTime scheduledAt,
        int? durationMinutes, string? location, string? interviewers, string? notes)
    {
        JobApplicationId = jobApplicationId;
        Type = type;
        ScheduledAt = scheduledAt;
        DurationMinutes = durationMinutes;
        Location = location;
        Interviewers = interviewers;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkFollowUpSent()
    {
        FollowUpSentAt = DateTime.UtcNow;
    }

    public void Reschedule(DateTime newDate)
    {
        ScheduledAt = newDate;
    }
}
