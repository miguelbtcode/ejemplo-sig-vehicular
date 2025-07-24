namespace Identity.Permissions.Services;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string module, string permission);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
    Task<List<string>> GetUserRolesAsync(Guid userId);
}
