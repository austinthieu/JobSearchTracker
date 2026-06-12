using Domain.Common;

namespace Domain.Entities;

public class Company : Entity
{
  public string Name { get; private set; } = null!;
  public string? Website { get; private set; }
  public string? Notes { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  // Navigation
  public ICollection<JobApplication> JobApplications { get; private set; } = [];

  private Company() { }

  public Company(string name, string? website, string? notes)
  {
    Name = name;
    Website = website;
    Notes = notes;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  public void Update(string name, string? website, string? notes)
  {
    Name = name;
    Website = website;
    Notes = notes;
    UpdatedAt = DateTime.UtcNow;
  }
}
