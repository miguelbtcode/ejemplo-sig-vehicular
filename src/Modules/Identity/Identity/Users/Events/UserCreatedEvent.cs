using Shared.Contracts.DDD;

namespace Identity.Users.Events;

public record UserCreatedEvent(User Usuario) : IDomainEvent;
