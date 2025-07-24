using Identity.Authorization.Extensions;
using Identity.Roles.Dtos;

namespace Identity.Roles.Features.UpdateRole;

public record UpdateRoleRequest(UpdateRoleDto Role);

public record UpdateRoleResponse(bool IsSuccess);

public class UpdateRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/identity/roles/{id:guid}",
                async (Guid id, UpdateRoleRequest request, ISender sender) =>
                {
                    var command = new UpdateRoleCommand(id, request.Role);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<UpdateRoleResponse>();

                    return Results.Ok(response);
                }
            )
            .WithTags("Roles")
            .WithName("UpdateRole")
            .Produces<UpdateRoleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Update Role")
            .WithDescription("Update role information and permissions")
            .RequireAdmin();
    }
}
