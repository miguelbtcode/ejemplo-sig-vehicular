using Identity.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Identity.Authorization.Handlers;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        var requiredPermission = $"{requirement.Module}:{requirement.Permission}";

        if (context.User.HasClaim("permission", requiredPermission))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
