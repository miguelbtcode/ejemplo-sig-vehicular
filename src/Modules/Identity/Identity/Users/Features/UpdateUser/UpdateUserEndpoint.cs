using Identity.Users.Dtos;

namespace Identity.Users.Features.UpdateUser;

public record UpdateUserRequest(UpdateUserDto Usuario);

public record UpdateUserResponse(bool IsSuccess);

public class UpdateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/identity/users",
                async (UpdateUserRequest request, ISender sender) =>
                {
                    var command = new UpdateUserCommand(request.Usuario);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<UpdateUserResponse>();

                    return Results.Ok(response);
                }
            )
            .WithName("UpdateUser")
            .Produces<UpdateUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update User")
            .WithDescription("Update user information");
    }
}
