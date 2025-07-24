using Shared.Contracts.DDD;

namespace Identity.Users.Events;

public record UserUpdatedEvent(User User) : IDomainEvent;
