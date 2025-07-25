namespace Identity.Users.Events;

public record UserDeletedEvent(Guid UserId) : IDomainEvent;
