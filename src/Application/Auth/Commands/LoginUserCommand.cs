using Application.Common.Interfaces;
using Application.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
  private readonly IApplicationDbContext _context;
  private readonly ITokenService _tokenService;
  private readonly PasswordHasher<User> _hasher;

  public LoginUserHandler(IApplicationDbContext context, ITokenService tokenService)
  {
    _context = context;
    _tokenService = tokenService;
    _hasher = new PasswordHasher<User>();
  }

  public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken ct)
  {
    var normalizedEmail = request.Email.Trim().ToLowerInvariant();

    var user = await _context.Users
      .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, ct)
      ?? throw new UnauthorizedAccessException("Invalid credentials");

    var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
    if (result == PasswordVerificationResult.Failed)
      throw new UnauthorizedAccessException("Invalid Credentials");

    if (result == PasswordVerificationResult.SuccessRehashNeeded)
    {
      user.SetPassword(_hasher.HashPassword(user, request.Password));
      await _context.SaveChangesAsync(ct);
    }

    return new AuthResponse
    {
      Token = _tokenService.GenerateToken(user.Id, user.Email),
      UserId = user.Id,
      Email = user.Email,
      Name = user.Name,
      ExpiresAt = DateTime.UtcNow.AddDays(7)
    };

  }
}
