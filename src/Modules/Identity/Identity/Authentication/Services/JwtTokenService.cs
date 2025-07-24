using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Authentication.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Authentication.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public JwtTokenResult GenerateToken(
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
        };

        // Agregar roles
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Agregar permisos
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var expiry = DateTime.UtcNow.AddHours(
            double.Parse(configuration["Jwt:ExpiryHours"] ?? "24")
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
}
