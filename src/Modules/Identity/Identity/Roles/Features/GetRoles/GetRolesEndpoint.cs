using Identity.Authorization.Extensions;

namespace Identity.Roles.Features.GetRoles;

public record GetRolesResponse(RoleStatistics Statistics, List<RoleDetailDto> Roles);

public class GetRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/identity/roles",
                async (ISender sender) =>
                {
                    var result = await sender.SendAsync(new GetRolesQuery());
                    var response = result.Adapt<GetRolesResponse>();

                    return Results.Ok(response);
                }
            )
            .WithName("GetRoles")
            .Produces<GetRolesResponse>(StatusCodes.Status200OK)
            .WithSummary("Get Roles")
            .WithDescription("Get list of roles")
            .RequireAdmin();
        ;
    }
}
