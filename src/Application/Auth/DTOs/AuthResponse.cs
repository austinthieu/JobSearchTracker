namespace Application.Auth.DTOs;

public class AuthResponse
{
  public string Token { get; set; } = null!;
  public Guid UserId { get; set; }
  public string Email { get; set; } = null!;
  public string? Name { get; set; }
  public DateTime ExpiresAt { get; set; }
}
