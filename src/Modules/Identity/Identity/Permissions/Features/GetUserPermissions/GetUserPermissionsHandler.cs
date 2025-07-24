using Identity.Permissions.Dtos;

namespace Identity.Permissions.Features.GetUserPermissions;

public record GetUserPermissionsQuery(Guid UserId) : IQuery<GetUserPermissionsResult>;

public record GetUserPermissionsResult(List<PermisoDto> Permissions);

internal class GetUserPermissionsHandler(IdentityDbContext dbContext)
    : IQueryHandler<GetUserPermissionsQuery, GetUserPermissionsResult>
{
    public async Task<GetUserPermissionsResult> HandleAsync(
        GetUserPermissionsQuery query,
        CancellationToken cancellationToken
    )
    {
        var permissions = await dbContext
            .Users.Where(u => u.Id == query.UserId)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role.Permissions)
            .Include(p => p.Role)
            .Include(p => p.Module)
            .Include(p => p.PermissionType)
            .AsNoTracking()
            .Select(p => new PermisoDto(
                p.Id,
                p.Role.Name,
                p.Module.Name,
                p.PermissionType.Name,
                p.DateAssigned
            ))
            .ToListAsync(cancellationToken);

        return new GetUserPermissionsResult(permissions);
    }
}
