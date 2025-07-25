using System.Security.Claims;
using Identity.Authentication.ValueObjects;

namespace Identity.Authentication.Services;

public interface ITokenService
{
    TokenPair GenerateTokenPair(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        DeviceInfo device
    );
    ClaimsPrincipal? ValidateAccessToken(string token);
    TokenInfo? GetTokenInfo(string token);
    bool IsTokenNearExpiry(string token, TimeSpan threshold);
}
