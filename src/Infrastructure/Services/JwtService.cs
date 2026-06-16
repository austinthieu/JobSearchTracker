using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;


public class JwtService : ITokenService
{
  private readonly string _key;
  private readonly string _issuer;
  private readonly string _audience;

  public JwtService(IConfiguration configuration)
  {
    _key = configuration["Jwt:Key"]!;
    _issuer = configuration["Jwt:Issuer"]!;
    _audience = configuration["Jwt:Audience"]!;
  }

  public string GenerateToken(Guid userId, string email)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

    var token = new JwtSecurityToken(
        _issuer,
        _audience,
        claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
