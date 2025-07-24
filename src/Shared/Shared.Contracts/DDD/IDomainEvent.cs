namespace Shared.Contracts.DDD;

public interface IDomainEvent
{
    Guid EventId => Guid.NewGuid();
    DateTime OccurredOn => DateTime.Now;
    string EventType => GetType().AssemblyQualifiedName!;
}
