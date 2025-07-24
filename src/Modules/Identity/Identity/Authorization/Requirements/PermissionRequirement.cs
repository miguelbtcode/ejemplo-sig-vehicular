using Microsoft.AspNetCore.Authorization;

namespace Identity.Authorization.Requirements;

public class PermissionRequirement(string module, string permission) : IAuthorizationRequirement
{
    public string Module { get; } = module;
    public string Permission { get; } = permission;
}
