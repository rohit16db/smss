using SMS.Domain.Entities;

namespace SMS.Domain.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
