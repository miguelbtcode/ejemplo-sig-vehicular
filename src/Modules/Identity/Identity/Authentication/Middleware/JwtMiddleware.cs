using Identity.Authentication.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Authentication.Middleware;

public class JwtMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var jwtTokenService = context.RequestServices.GetRequiredService<IJwtTokenService>();
            var principal = jwtTokenService.ValidateToken(token);

            if (principal != null)
            {
                context.User = principal;
            }
        }

        await next(context);
    }
}
