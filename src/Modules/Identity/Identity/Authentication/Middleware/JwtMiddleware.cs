using Identity.Authentication.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Authentication.Middleware;

public class JwtMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractToken(context.Request);

        if (!string.IsNullOrEmpty(token))
        {
            var tokenService = context.RequestServices.GetRequiredService<ITokenService>();
            var principal = tokenService.ValidateAccessToken(token);

            if (principal != null)
            {
                context.User = principal;
            }
        }

        await next(context);
    }

    private static string? ExtractToken(HttpRequest request)
    {
        // Authorization header
        var authHeader = request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
            return authHeader[7..];

        // Query string (SignalR)
        return request.Query["access_token"];
    }
}
