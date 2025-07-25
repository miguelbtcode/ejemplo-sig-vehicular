namespace Identity.Authentication.Features.RefreshToken;

public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenResponse(string AccessToken, DateTime ExpiresAt);

public class RefreshTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/identity/auth/refresh",
                async (RefreshTokenRequest request, ISender sender) =>
                {
                    var command = new RefreshTokenCommand(request.RefreshToken);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<RefreshTokenResponse>();

                    return Results.Ok(response);
                }
            )
            .WithTags("Authentication")
            .WithName("RefreshToken")
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Refresh Access Token")
            .WithDescription("Generate new access token using refresh token");
    }
}
