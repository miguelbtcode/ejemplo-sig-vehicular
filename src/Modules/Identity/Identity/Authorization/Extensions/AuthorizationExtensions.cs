namespace Identity.Authorization.Extensions;

public static class AuthorizationExtensions
{
    public static RouteHandlerBuilder RequireAdmin(this RouteHandlerBuilder builder)
    {
        return builder.RequireAuthorization("AdminOnly");
    }

    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        string module,
        string permission
    )
    {
        return builder.RequireAuthorization($"{module}.{permission}");
    }
}
