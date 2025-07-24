using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.DDD;

namespace Shared.DDD.Dispatchers;

public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        var tasks = handlers
            .Cast<object>()
            .Select(handler =>
            {
                var method = handlerType.GetMethod(
                    nameof(IDomainEventHandler<IDomainEvent>.HandleAsync)
                )!;
                return (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
            });

        await Task.WhenAll(tasks);
    }
}
