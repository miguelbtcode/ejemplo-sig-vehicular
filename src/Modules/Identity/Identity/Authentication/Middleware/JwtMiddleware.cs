using Identity.Authentication.Services;

namespace Identity.Authentication.Middleware;

public class JwtMiddleware(RequestDelegate next, IJwtTokenService jwtTokenService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var principal = jwtTokenService.ValidateToken(token);
            if (principal != null)
            {
                context.User = principal;
            }
        }

        await next(context);
    }
}
