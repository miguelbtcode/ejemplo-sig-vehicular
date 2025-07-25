namespace Identity.Users.Features.CreateUser;

public record CreateUserRequest(CreateUserDto User);

public record CreateUserResponse(Guid Id);

public class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/identity/users",
                async (CreateUserRequest request, ISender sender) =>
                {
                    var command = new CreateUserCommand(request.User);
                    var result = await sender.SendAsync(command);
                    var response = result.Value.Adapt<CreateUserResponse>();

                    return Results.Created($"/identity/users/{response.Id}", response);
                }
            )
            .WithTags("Users")
            .WithName("CreateUser")
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create User")
            .WithDescription("Create a new user")
            .RequireAdmin();
    }
}
