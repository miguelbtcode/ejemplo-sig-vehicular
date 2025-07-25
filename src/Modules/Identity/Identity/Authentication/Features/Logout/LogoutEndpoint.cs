namespace Identity.Authentication.Features.Logout;

public record LogoutRequest(string RefreshToken, bool LogoutAllDevices = false);

public record LogoutResponse(bool Success);

public class LogoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/identity/auth/logout",
                async (LogoutRequest request, ISender sender) =>
                {
                    var command = new LogoutCommand(request.RefreshToken, request.LogoutAllDevices);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<LogoutResponse>();

                    return Results.Ok(response);
                }
            )
            .WithTags("Authentication")
            .WithName("Logout")
            .Produces<LogoutResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("User Logout")
            .WithDescription("Revoke refresh token and logout user")
            .RequireAuthorization();
    }
}
