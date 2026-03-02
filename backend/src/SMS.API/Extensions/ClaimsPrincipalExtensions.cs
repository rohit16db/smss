using System.Security.Claims;

namespace SMS.API.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to extract user information from JWT claims
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from HTTP context</param>
    /// <returns>User ID as GUID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID claim is not found or invalid</exception>
    public static Guid GetCurrentUserId(this ClaimsPrincipal user)
    {
        if (user == null)
        {
            throw new UnauthorizedAccessException("User principal is null");
        }

        // JWT claims use standardClaimTypes.NameIdentifier for user ID
        // Try multiple approaches due to claim type transformations in JWT parsing
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value
            ?? user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            var claimsDebug = string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}"));
            throw new UnauthorizedAccessException($"User ID claim not found in JWT token. Available claims: {claimsDebug}");
        }
        
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException($"Invalid user ID format in JWT token: {userIdClaim}. Expected a valid GUID.");
        }

        return userId;
    }

    /// <summary>
    /// Gets the current username from JWT claims
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from HTTP context</param>
    /// <returns>Username string</returns>
    public static string? GetCurrentUsername(this ClaimsPrincipal user)
    {
        return user?.FindFirst(ClaimTypes.Name)?.Value
            ?? user?.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Gets the current user's role from JWT claims
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from HTTP context</param>
    /// <returns>Role string</returns>
    public static string? GetCurrentRole(this ClaimsPrincipal user)
    {
        return user?.FindFirst(ClaimTypes.Role)?.Value;
    }
}
