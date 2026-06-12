namespace Application.JobApplications.DTOs;

public class ApplicationDto
{
  public Guid Id { get; set; }
  public string Position { get; set; } = null!;
  public decimal? SalaryMin { get; set; }
  public decimal? SalaryMax { get; set; }
  public string? JobUrl { get; set; }
  public string? Source { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
