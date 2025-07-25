using Identity.Authentication.Dtos.Common;
using Identity.Authentication.Extensions;

namespace Identity.Authentication.Features.GetActiveSessions;

public record GetActiveSessionsRequest();

public record GetActiveSessionsResponse(List<DeviceSessionDto> Sessions);

public class GetActiveSessionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/identity/auth/sessions",
                async (ISender sender, HttpContext context) =>
                {
                    var userId = context.GetUserId();
                    if (!userId.HasValue)
                        return Results.Unauthorized();

                    var result = await sender.SendAsync(new GetActiveSessionsQuery(userId.Value));
                    var response = result.Adapt<GetActiveSessionsResponse>();

                    return Results.Ok(response);
                }
            )
            .WithTags("Authentication")
            .WithName("GetActiveSessions")
            .Produces<GetActiveSessionsResponse>(StatusCodes.Status200OK)
            .WithSummary("Get User Active Sessions")
            .WithDescription("Get all active sessions for the current user")
            .RequireAuthorization();
    }
}
