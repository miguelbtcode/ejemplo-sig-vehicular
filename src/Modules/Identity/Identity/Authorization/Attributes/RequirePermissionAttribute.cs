using Microsoft.AspNetCore.Authorization;

namespace Identity.Authorization.Attributes;

public class RequirePermissionAttribute(string module, string permission) : AuthorizeAttribute
{
    public string Module { get; } = module;
    public string Permission { get; } = permission;
    public string PolicyName { get; } = $"Permission.{module}.{permission}";
}
