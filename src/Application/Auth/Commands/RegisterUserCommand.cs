using Application.Common.Interfaces;
using Application.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands;

public record RegisterUserCommand(string Email, string Password, string? Name) : IRequest<AuthResponse>;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
  private readonly IApplicationDbContext _context;
  private readonly ITokenService _tokenService;
  private readonly PasswordHasher<User> _hasher;

  public RegisterUserHandler(IApplicationDbContext context, ITokenService tokenService)
  {
    _context = context;
    _tokenService = tokenService;
    _hasher = new PasswordHasher<User>();
  }

  public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken ct)
  {
    var normalizedEmail = request.Email.Trim().ToLowerInvariant();

    var exists = await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, ct);
    if (exists)
      throw new InvalidOperationException("Email already registered");

    var user = new User(request.Email, normalizedEmail, string.Empty, request.Name);
    var passwordHash = _hasher.HashPassword(user, request.Password);
    user.SetPassword(passwordHash);

    _context.Users.Add(user);
    await _context.SaveChangesAsync(ct);

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
