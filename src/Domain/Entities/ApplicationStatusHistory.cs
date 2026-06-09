using Domain.Enums;
using Domain.Common;

namespace Domain.Entities;

public class ApplicationStatusHistory : Entity
{
    public ApplicationStatus FromStatus { get; private set; }
    public ApplicationStatus ToStatus { get; private set; }
    public string? Note { get; private set; }
    public DateTime ChangedAt { get; private set; }

    // Foreign Keys
    public Guid JobApplicationId { get; private set; }

    // Navigation
    public JobApplication JobApplication { get; private set; } = null!;

    private ApplicationStatusHistory() { }

    public ApplicationStatusHistory(Guid jobApplicationId, ApplicationStatus fromStatus, ApplicationStatus toStatus, string? note)
    {
        JobApplicationId = jobApplicationId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Note = note;
        ChangedAt = DateTime.UtcNow;
    }
}
