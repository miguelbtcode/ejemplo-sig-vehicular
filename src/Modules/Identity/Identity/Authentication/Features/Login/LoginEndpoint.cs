using Identity.Authentication.Dtos;

namespace Identity.Authentication.Features.Login;

public record LoginRequest(LoginRequestDto LoginData);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    DateTime RefreshTokenExpiry,
    UserDto User,
    List<DeviceSessionDto> ActiveSessions,
    string SessionType
);

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/identity/login",
                async (LoginRequest request, ISender sender) =>
                {
                    var command = new LoginCommand(request.LoginData);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Response.Adapt<LoginResponse>();

                    return Results.Ok(response);
                }
            )
            .WithTags("Authentication")
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("User Login")
            .WithDescription("Authenticate user and return JWT token");
    }
}
