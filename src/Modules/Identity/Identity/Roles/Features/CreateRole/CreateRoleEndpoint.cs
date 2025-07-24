using Identity.Roles.Dtos;

namespace Identity.Roles.Features.CreateRole;

public record CreateRoleRequest(CreateRoleDto Role);

public record CreateRoleResponse(Guid Id);

public class CreateRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/identity/roles",
                async (CreateRoleRequest request, ISender sender) =>
                {
                    var command = new CreateRoleCommand(request.Role);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<CreateRoleResponse>();

                    return Results.Created($"/identity/roles/{response.Id}", response);
                }
            )
            .WithName("CreateRole")
            .Produces<CreateRoleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Create Role")
            .WithDescription("Create a new role with specified permissions");
    }
}
