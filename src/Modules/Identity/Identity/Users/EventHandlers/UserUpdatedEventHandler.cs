using Microsoft.Extensions.Logging;
using Shared.Contracts.DDD;

namespace Identity.Users.EventHandlers;

public class UserUpdatedEventHandler(
    IdentityDbContext dbContext,
    ILogger<UserUpdatedEventHandler> logger
) : IDomainEventHandler<UserUpdatedEvent>
{
    public async Task HandleAsync(
        UserUpdatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "User updated: {UserId} - {UserName}",
            notification.User.Id,
            notification.User.Name
        );

        try
        {
            await SynchronizeUserRoles(
                notification.User.Id,
                notification.NewRoleIds,
                cancellationToken
            );

            logger.LogInformation(
                "Successfully synchronized roles for user: {UserId}",
                notification.User.Id
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to synchronize roles for user: {UserId}",
                notification.User.Id
            );
            throw; // Re-throw para que la transacción falle
        }
    }

    private async Task SynchronizeUserRoles(
        Guid userId,
        List<Guid> newRoleIds,
        CancellationToken cancellationToken
    )
    {
        // Simple y efectivo - Replace All con EF
        var existingUserRoles = await dbContext
            .UserRoles.Where(ur => ur.IdUser == userId)
            .ToListAsync(cancellationToken);

        // Eliminar todos los existentes
        dbContext.UserRoles.RemoveRange(existingUserRoles);

        // Agregar todos los nuevos
        if (newRoleIds.Any())
        {
            var newUserRoles = newRoleIds
                .Select(roleId => UserRole.Create(userId, roleId))
                .ToList();

            dbContext.UserRoles.AddRange(newUserRoles);
        }

        logger.LogDebug(
            "Replaced {OldCount} roles with {NewCount} roles for user {UserId}",
            existingUserRoles.Count,
            newRoleIds.Count,
            userId
        );
    }
}
