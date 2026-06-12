namespace Application.JobApplications.DTOs;

public class ApplicationDetailDto
{
  public Guid Id { get; set; }
  public string Position { get; set; } = null!;
  public string? SalaryMin { get; set; }
  public string? SalaryMax { get; set; }
  public string? JobUrl { get; set; }
  public string? Source { get; set; }
  public string Status { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  // Company
  public Guid CompanyId { get; set; }
  public string CompanyName { get; set; } = null!;
  public string? CompanyWebsite { get; set; }

  // Related data
  public List<InterviewDto> Interviews { get; set; } = [];
  public List<ContactDto> Contacts { get; set; } = [];
  public List<NoteDto> Notes { get; set; } = [];
  public List<StatusHistoryDto> StatusHistory { get; set; } = [];
}

public class InterviewDto
{
  public Guid Id { get; set; }
  public string Type { get; set; } = null!;
  public DateTime ScheduledAt { get; set; }
  public int? DurationMinutes { get; set; }
  public string? Location { get; set; }
  public string? Interviewers { get; set; }
  public string? Notes { get; set; }
  public DateTime? FollowUpSentAt { get; set; }
}

public class ContactDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Email { get; set; }
  public string? Phone { get; set; }
  public string? Title { get; set; }
  public string? Notes { get; set; }
}

public class NoteDto
{
  public Guid Id { get; set; }
  public string Content { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
}

public class StatusHistoryDto
{
  public string FromStatus { get; set; } = null!;
  public string ToStatus { get; set; } = null!;
  public string? Note { get; set; }
  public DateTime ChangedAt { get; set; }
}
