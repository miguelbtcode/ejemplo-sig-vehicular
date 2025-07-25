namespace Identity.Users.Events;

public record UserUpdatedEvent(User User, List<Guid> NewRoleIds) : IDomainEvent;
