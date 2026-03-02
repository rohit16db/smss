using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SMS.Domain.Entities;
using SMS.Domain.Interfaces;

namespace SMS.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryHours;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secret = configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET not configured");
        _issuer = configuration["JWT_ISSUER"] ?? "SchoolManagementSystem";
        _audience = configuration["JWT_AUDIENCE"] ?? "SMSWebClient";
        _expiryHours = int.Parse(configuration["JWT_EXPIRY_HOURS"] ?? "8");
    }

    public string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            //new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString("G")), // Use "G" format to get the enum name instead of numeric value
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
            ValidateLifetime = false, // Don't validate lifetime for expired token
            ValidIssuer = _issuer,
            ValidAudience = _audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
