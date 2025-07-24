using Microsoft.Extensions.Logging;
using Shared.Contracts.DDD;

namespace Identity.Users.EventHandlers;

public class UserUpdatedEventHandler(ILogger<UserUpdatedEventHandler> logger)
    : IDomainEventHandler<UserUpdatedEvent>
{
    public Task HandleAsync(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User updated: {UserId} - {UserName}",
            notification.User.Id,
            notification.User.Name
        );

        return Task.CompletedTask;
    }
}
