using MediatR;
using SMS.Application.Auth.DTOs;

namespace SMS.Application.Auth.Commands;

public class LoginCommand : IRequest<AuthResponse>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterCommand : IRequest<AuthResponse>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Domain.Enums.UserRole Role { get; set; }
}

public class RefreshTokenCommand : IRequest<AuthResponse>
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
