using Identity.Authorization.Requirements;
using Identity.Permissions.Services;
using Microsoft.AspNetCore.Authorization;

namespace Identity.Authorization.Handlers;

public class PermissionAuthorizationHandler(IPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        var userIdClaim = context.User.FindFirst("user_id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        var hasPermission = await permissionService.UserHasPermissionAsync(
            userId,
            requirement.Module,
            requirement.Permission
        );

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
