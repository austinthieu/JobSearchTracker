using Domain.Enums;
using Domain.Common;

namespace Domain.Entities;

public class JobApplication : Entity
{
    public string Position { get; private set; } = null!;
    public ApplicationStatus Status { get; private set; }
    public decimal? SalaryMin { get; private set; }
    public decimal? SalaryMax { get; private set; }
    public string? JobUrl { get; private set; }
    public string? Source { get; private set; } // LinkedIn, INdeed, referral, etc.
    public DateOnly? AppliedDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Foreign Keys
    public Guid CompanyId { get; private set; }

    // Navigation
    public Company Company { get; private set; } = null!;
    public ICollection<Interview> Interviews { get; private set; } = [];
    public ICollection<Contact> Contacts { get; private set; } = [];
    public ICollection<Attachment> Attachments { get; private set; } = [];
    public ICollection<ApplicationNote> Notes { get; private set; } = [];
    public ICollection<ApplicationStatusHistory> StatusHistory { get; private set; } = [];

    private JobApplication() { } // For EF Core

    public JobApplication(string position, Guid companyId, string? jobUrl, string? source)
    {
        Position = position;
        CompanyId = companyId;
        JobUrl = jobUrl;
        Source = source;
        Status = ApplicationStatus.Saved;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ApplicationStatus newStatus)
    {
        if (Status == newStatus) return;
        // Could add domain event here later
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

}
