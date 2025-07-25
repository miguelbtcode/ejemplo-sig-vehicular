using Identity.Authentication.Extensions;

namespace Identity.Users.Features.ChangePassword;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ChangePasswordResponse(bool IsSuccess);

public class ChangePasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/identity/auth/change-password",
                async (ChangePasswordRequest request, ISender sender, HttpContext context) =>
                {
                    var userId = context.GetUserId();
                    if (!userId.HasValue)
                        return Results.Unauthorized();

                    var command = new ChangePasswordCommand(
                        userId.Value,
                        request.CurrentPassword,
                        request.NewPassword
                    );
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<ChangePasswordResponse>();

                    return Results.Ok(response);
                }
            )
            .WithTags("Authentication")
            .WithName("ChangePassword")
            .Produces<ChangePasswordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Change Password")
            .WithDescription("Change user password")
            .RequireAuthorization();
    }
}
