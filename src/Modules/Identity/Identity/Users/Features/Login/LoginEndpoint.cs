using Identity.Users.Dtos;

namespace Identity.Users.Features.Login;

public record LoginRequest(LoginDto LoginData);

public record LoginResponse(
    string? Token,
    DateTime? Expiry,
    string? UserName,
    List<string> Roles,
    List<string> Permissions
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
                    var response = result.Value.AuthResult.Adapt<LoginResponse>();

                    return Results.Ok(response);
                }
            )
            .WithName("Login")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("User Login")
            .WithDescription("Authenticate user and return JWT token");
    }
}
