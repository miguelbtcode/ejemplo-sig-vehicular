using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Authentication.Configuration;
using Identity.Authentication.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Authentication.Services.Implementation;

public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;
    private readonly SymmetricSecurityKey _signingKey = new(
        Encoding.UTF8.GetBytes(jwtOptions.Value.Key)
    );

    public TokenPair GenerateTokenPair(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        DeviceInfo device
    )
    {
        var (Token, Expiry) = GenerateAccessToken(user, roles, permissions);
        var refreshToken = GenerateRefreshToken();

        var refreshExpiry = device.IsMobile
            ? DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenMobileExpiryDays)
            : DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        return new TokenPair(Token, refreshToken, Expiry, refreshExpiry);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = CreateValidationParameters();

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    public TokenInfo? GetTokenInfo(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
            var jwtId = jwt.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
            var userName = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return new TokenInfo(
                jwtId,
                Guid.TryParse(userIdClaim, out var userId) ? userId : null,
                jwt.ValidTo
            );
        }
        catch
        {
            return null;
        }
    }

    public bool IsTokenNearExpiry(string token, TimeSpan threshold)
    {
        var tokenInfo = GetTokenInfo(token);
        return tokenInfo?.IsNearExpiry(threshold) ?? true;
    }

    private (string Token, DateTime Expiry) GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions
    )
    {
        var claims = CreateClaims(user, roles, permissions);
        var expiry = DateTime.UtcNow.AddHours(GetAccessTokenExpiryHours());

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static List<Claim> CreateClaims(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions
    )
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("user_id", user.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        return claims;
    }

    private TokenValidationParameters CreateValidationParameters() =>
        new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.Zero,
        };

    private int GetAccessTokenExpiryHours() => _jwtSettings.ExpiryHours;
}
