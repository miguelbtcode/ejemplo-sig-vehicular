namespace Identity.Roles.Features.GetRoles;

public record GetRolesQuery() : IQuery<GetRolesResult>;

public record GetRolesResult(RoleStatistics Statistics, List<RoleDetailDto> Roles);

public record RoleStatistics(
    int TotalRoles,
    int AssignedUsers,
    int AvailableModules,
    int TotalPermissions
);

public record RoleDetailDto(
    Guid Id,
    string Name,
    string Description,
    bool Enabled,
    int AssignedUsers,
    int ModulesCount,
    int PermissionsCount,
    List<ModuleWithPermissionsDto> ModulesWithPermissions
);

public record ModuleWithPermissionsDto(string ModuleName, List<string> Permissions);

internal class GetRolesHandler(IdentityDbContext dbContext)
    : IQueryHandler<GetRolesQuery, GetRolesResult>
{
    public async Task<GetRolesResult> HandleAsync(
        GetRolesQuery query,
        CancellationToken cancellationToken
    )
    {
        // 1. Get total roles
        var totalRoles = await dbContext.Roles.CountAsync(r => r.Enabled, cancellationToken);

        // 2. Get total assigned users
        var totalAssignedUsers = await dbContext
            .UserRoles.Where(ur => ur.Role.Enabled)
            .Select(ur => ur.IdUser)
            .Distinct()
            .CountAsync(cancellationToken);

        // 3. Get total available modules
        var totalAvailableModules = await dbContext.Modules.CountAsync(
            r => r.Enabled,
            cancellationToken
        );

        // 4. Get total permissions
        var totalPermissions = await dbContext
            .Permissions.Where(p => p.Role.Enabled)
            .CountAsync(cancellationToken);

        // 5. Create the statistics object
        var statistics = new RoleStatistics(
            totalRoles,
            totalAssignedUsers,
            totalAvailableModules,
            totalPermissions
        );

        // 6. Get roles with details
        var rolesData = await dbContext
            .Roles.AsNoTracking()
            .Where(r => r.Enabled)
            .Include(r => r.UserRoles)
            .Include(r => r.Permissions)
            .ThenInclude(p => p.Module)
            .Include(r => r.Permissions)
            .ThenInclude(p => p.PermissionType)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        // 7. Build the role details response
        var roleDetails = rolesData
            .Select(role =>
            {
                // 7.1. Get assigned users count
                var assignedUsersCount = role.UserRoles.Count;

                // 7.2. Get module count available
                var modulePermissions = role
                    .Permissions.Where(p => p.Module.Enabled)
                    .GroupBy(p => p.Module.Name)
                    .Select(g => new ModuleWithPermissionsDto(
                        g.Key,
                        g.Select(p => p.PermissionType.Name).OrderBy(name => name).ToList()
                    ))
                    .OrderBy(mp => mp.ModuleName)
                    .ToList();

                var modulesCount = modulePermissions.Count;
                var permissionsCount = role.Permissions.Count(p => p.Module.Enabled);

                return new RoleDetailDto(
                    role.Id,
                    role.Name,
                    role.Description,
                    role.Enabled,
                    assignedUsersCount,
                    modulesCount,
                    permissionsCount,
                    modulePermissions
                );
            })
            .ToList();

        return new GetRolesResult(statistics, roleDetails);
    }
}
