using System.Security.Claims;
using Identity.Authentication.Models;

namespace Identity.Authentication.Services;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions
    );
    ClaimsPrincipal? ValidateToken(string token);
}
