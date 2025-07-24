using Microsoft.Extensions.Logging;
using Shared.Contracts.DDD;

namespace Identity.Users.EventHandlers;

public class UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    : IDomainEventHandler<UserCreatedEvent>
{
    public Task HandleAsync(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User created: {UserId} - {UserName}",
            notification.Usuario.Id,
            notification.Usuario.Name
        );

        return Task.CompletedTask;
    }
}
