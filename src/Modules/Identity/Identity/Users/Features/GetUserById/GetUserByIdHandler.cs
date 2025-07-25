namespace Identity.Users.Features.GetUserById;

public record GetUserByIdQuery(Guid Id) : IQuery<Result<GetUserByIdResult>>;

public record GetUserByIdResult(UserDto UserDto);

internal class GetUserByIdHandler(IdentityDbContext dbContext)
    : IQueryHandler<GetUserByIdQuery, Result<GetUserByIdResult>>
{
    public async Task<Result<GetUserByIdResult>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.Id, cancellationToken);

        if (user == null)
            return UserErrors.NotFound(query.Id);

        var userDto = new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.CreatedAt!.Value,
            user.Enabled,
            user.UserRoles.Select(ur => ur.Role.Name).ToList()
        );

        return new GetUserByIdResult(userDto);
    }
}
