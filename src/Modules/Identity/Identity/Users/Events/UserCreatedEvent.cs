namespace Identity.Users.Events;

public record UserCreatedEvent(User Usuario) : IDomainEvent;
