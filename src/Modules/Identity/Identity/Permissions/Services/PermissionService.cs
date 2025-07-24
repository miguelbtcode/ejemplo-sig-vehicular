namespace Identity.Permissions.Services;

public class PermissionService(IdentityDbContext dbContext) : IPermissionService
{
    public async Task<bool> UserHasPermissionAsync(Guid userId, string module, string permission)
    {
        return await dbContext
            .Users.Where(u => u.Id == userId && u.Enabled)
            .SelectMany(u => u.UserRoles)
            .Where(ur => ur.Role.Enabled)
            .SelectMany(ur => ur.Role.Permissions)
            .AnyAsync(p => p.Module.Name == module && p.PermissionType.Name == permission);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        return await dbContext
            .Users.Where(u => u.Id == userId && u.Enabled)
            .SelectMany(u => u.UserRoles)
            .Where(ur => ur.Role.Enabled)
            .SelectMany(ur => ur.Role.Permissions)
            .Select(p => $"{p.Module.Name}:{p.PermissionType.Name}")
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await dbContext
            .Users.Where(u => u.Id == userId && u.Enabled)
            .SelectMany(u => u.UserRoles)
            .Where(ur => ur.Role.Enabled)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }
}
