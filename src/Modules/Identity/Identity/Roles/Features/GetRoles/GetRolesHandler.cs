using Identity.Roles.Dtos;

namespace Identity.Roles.Features.GetRoles;

public record GetRolesQuery() : IQuery<GetRolesResult>;

public record GetRolesResult(List<RoleDto> Roles);

internal class GetRolesHandler(IdentityDbContext dbContext)
    : IQueryHandler<GetRolesQuery, GetRolesResult>
{
    public async Task<GetRolesResult> HandleAsync(
        GetRolesQuery query,
        CancellationToken cancellationToken
    )
    {
        var roles = await dbContext
            .Roles.AsNoTracking()
            .Where(r => r.Enabled)
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(r.Id, r.Name, r.Description, r.Enabled))
            .ToListAsync(cancellationToken);

        return new GetRolesResult(roles);
    }
}
