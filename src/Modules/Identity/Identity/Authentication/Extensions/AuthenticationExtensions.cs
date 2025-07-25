using System.Security.Claims;

namespace Identity.Authentication.Extensions;

public static class AuthenticationExtensions
{
    public static RouteHandlerBuilder RequireValidToken(this RouteHandlerBuilder builder) =>
        builder.RequireAuthorization("UserOnly");

    public static ClaimsPrincipal GetCurrentUser(this HttpContext context) => context.User;

    public static Guid? GetUserId(this HttpContext context)
    {
        var userIdClaim = context.User.FindFirst("user_id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public static string? GetUserName(this HttpContext context) =>
        context.User.FindFirst(ClaimTypes.Name)?.Value;

    public static List<string> GetUserRoles(this HttpContext context) =>
        context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

    public static List<string> GetUserPermissions(this HttpContext context) =>
        context.User.FindAll("permission").Select(c => c.Value).ToList();

    public static bool HasPermission(this HttpContext context, string module, string permission) =>
        context.User.HasClaim("permission", $"{module}:{permission}");
}
