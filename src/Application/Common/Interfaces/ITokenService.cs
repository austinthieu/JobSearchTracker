namespace Application.Common.Interfaces;

public interface ITokenService
{
  string GenerateToken(Guid UserId, string Email);
}
