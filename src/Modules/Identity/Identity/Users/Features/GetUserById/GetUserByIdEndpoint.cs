namespace Identity.Users.Features.GetUserById;

public record GetUserByIdResponse(UserDto UserDto);

public class GetUserByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/identity/users/{id:int}",
                async (Guid id, ISender sender) =>
                {
                    var result = await sender.SendAsync(new GetUserByIdQuery(id));
                    var response = result.Value.Adapt<GetUserByIdResponse>();

                    return Results.Ok(response);
                }
            )
            .WithTags("Users")
            .WithName("GetUserById")
            .Produces<GetUserByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get User By Id")
            .WithDescription("Get user details by ID")
            .RequireAdmin();
    }
}
