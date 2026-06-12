using Domain.Common;

namespace Domain.Entities;

public class User : Entity
{
  public string Email { get; private set; } = null!;
  public string PasswordHash { get; private set; } = null!;
  public string? Name { get; private set; }
  public DateTime CreatedAt { get; private set; }

  // Navigation
  public ICollection<Company> Companies { get; private set; } = [];
  public ICollection<JobApplication> JobApplications { get; private set; } = [];

  private User() { }

  public User(string email, string passwordHash, string? name)
  {
    Email = email;
    PasswordHash = passwordHash;
    Name = name;
    CreatedAt = DateTime.UtcNow;
  }
}
