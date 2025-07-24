using Mapster;

namespace Identity.Users.Features.DeleteUser;

public record DeleteUserResponse(bool IsSuccess);

public class DeleteUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/identity/users/{id:int}",
                async (Guid id, ISender sender) =>
                {
                    var command = new DeleteUserCommand(id);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<DeleteUserResponse>();

                    return Results.Ok(response);
                }
            )
            .WithName("DeleteUser")
            .Produces<DeleteUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete User")
            .WithDescription("Deactivate user (soft delete)");
    }
}
