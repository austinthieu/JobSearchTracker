using Domain.Common;

namespace Domain.Entities;

public class Company : Entity
{
  public string Name { get; private set; } = null!;
  public string? Website { get; private set; }
  public string? Notes { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }
  public Guid UserId { get; private set; }
  public User User { get; private set; } = null!;

  // Navigation
  public ICollection<JobApplication> JobApplications { get; private set; } = [];

  private Company() { }

  public Company(string name, string? website, string? notes, Guid userId)
  {
    Name = name;
    Website = website;
    Notes = notes;
    UserId = userId;
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
