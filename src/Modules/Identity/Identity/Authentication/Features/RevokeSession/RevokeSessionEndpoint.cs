using Identity.Authentication.Extensions;

namespace Identity.Authentication.Features.RevokeSession;

public record RevokeSessionRequest(string DeviceId);

public record RevokeSessionResponse(bool Success);

public class RevokeSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/identity/auth/sessions/{deviceId}",
                async (string deviceId, ISender sender, HttpContext context) =>
                {
                    var userId = context.GetUserId();
                    if (!userId.HasValue)
                        return Results.Unauthorized();

                    var command = new RevokeSessionCommand(userId.Value, deviceId);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<RevokeSessionResponse>();

                    return Results.Ok(response);
                }
            )
            .WithTags("Authentication")
            .WithName("RevokeSession")
            .Produces<RevokeSessionResponse>(StatusCodes.Status200OK)
            .WithSummary("Revoke Session")
            .WithDescription("Revoke a specific user session")
            .RequireAuthorization();
    }
}
