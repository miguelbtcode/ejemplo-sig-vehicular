using System.Security.Claims;
using Identity.Authentication.Models;

namespace Identity.Authentication.Services;

public interface IJwtTokenService
{
    JwtTokenResult GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions
    );

    AuthenticationTokenResult GenerateTokens(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions
    );

    ClaimsPrincipal? ValidateToken(string token);

    // Utilities
    string? GetJwtIdFromToken(string token);
    Guid? GetUserIdFromToken(string token);
    bool IsTokenNearExpiry(string token, TimeSpan threshold);
    TokenInfo? GetTokenInfo(string token);
}
