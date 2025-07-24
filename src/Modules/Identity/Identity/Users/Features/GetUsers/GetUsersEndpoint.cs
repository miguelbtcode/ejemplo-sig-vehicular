using Identity.Users.Dtos;
using Shared.Pagination;

namespace Identity.Users.Features.GetUsers;

public record GetUsersResponse(PaginatedResult<UserDto> Users);

public class GetUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/identity/users",
                async ([AsParameters] PaginationRequest request, ISender sender) =>
                {
                    var result = await sender.SendAsync(new GetUsersQuery(request));
                    var response = new GetUsersResponse(result.Users);

                    return Results.Ok(response);
                }
            )
            .WithTags("Users")
            .WithName("GetUsers")
            .Produces<GetUsersResponse>(StatusCodes.Status200OK)
            .WithSummary("Get Users")
            .WithDescription("Get paginated list of users");
    }
}
