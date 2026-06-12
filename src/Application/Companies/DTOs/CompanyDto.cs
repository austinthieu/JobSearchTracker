namespace Application.Companies.DTOs;

public class CompanyDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Website { get; set; }
  public string? Notes { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
