using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Authentication.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Authentication.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public JwtTokenResult GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions
    )
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("user_id", user.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()), // JWT ID para invalidación/blacklist
        };

        // Agregar roles
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Agregar permisos
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        // Access tokens más cortos para mobile/web moderna
        var expiry = DateTime.UtcNow.AddHours(
            double.Parse(configuration["Jwt:ExpiryHours"] ?? "1") // Default 1 hora
        );

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return new JwtTokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiry = expiry,
            UserId = user.Id,
            UserName = user.Name,
            Email = user.Email,
            Roles = roles.ToList(),
            Permissions = permissions.ToList(),
        };
    }

    public AuthenticationTokenResult GenerateTokens(
        User user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions
    )
    {
        // 1. Generar access token
        var accessToken = GenerateAccessToken(user, roles, permissions);

        // 2. Crear refresh token (se persistirá en el handler)
        var refreshToken = RefreshToken.CreateMobile(
            user.Id,
            Guid.NewGuid().ToString(), // Temporal - se reemplaza en el handler
            "Unknown Device", // Temporal - se reemplaza en el handler
            "unknown", // Temporal - se reemplaza en el handler
            "1.0.0", // Temporal - se reemplaza en el handler
            DateTime.UtcNow.AddDays(60) // 60 días para mobile por defecto
        );

        return new AuthenticationTokenResult
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiry = accessToken.Expiry,
            RefreshTokenExpiry = refreshToken.ExpiresAt,
            UserId = user.Id,
            UserName = user.Name,
            Email = user.Email,
            Roles = roles.ToList(),
            Permissions = permissions.ToList(),
            RefreshTokenEntity = refreshToken,
        };
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero,
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    // NUEVO: Extraer JWT ID del token (para blacklist)
    public string? GetJwtIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
        }
        catch
        {
            return null;
        }
    }

    // NUEVO: Extraer User ID del token
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
        catch
        {
            return null;
        }
    }

    // NUEVO: Verificar si un token está próximo a expirar
    public bool IsTokenNearExpiry(string token, TimeSpan threshold)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);

            if (jwt.ValidTo == DateTime.MinValue)
                return true;

            var timeUntilExpiry = jwt.ValidTo.Subtract(DateTime.UtcNow);
            return timeUntilExpiry <= threshold;
        }
        catch
        {
            return true; // Si hay error, asumir que está expirado
        }
    }

    // NUEVO: Obtener información del token sin validar completamente
    public TokenInfo? GetTokenInfo(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
            var jwtId = jwt.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
            var userName = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return new TokenInfo
            {
                JwtId = jwtId,
                UserId = Guid.TryParse(userIdClaim, out var userId) ? userId : null,
                UserName = userName,
                IssuedAt = jwt.IssuedAt,
                ExpiresAt = jwt.ValidTo,
                IsExpired = jwt.ValidTo <= DateTime.UtcNow,
            };
        }
        catch
        {
            return null;
        }
    }
}
