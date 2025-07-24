using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.CQRS;

namespace Shared.CQRS.Dispatchers;

public class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TResponse> DispatchAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(
            command.GetType(),
            typeof(TResponse)
        );

        var handler = _serviceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod(
            nameof(ICommandHandler<ICommand<TResponse>, TResponse>.HandleAsync)
        );

        var task = (Task<TResponse>)method!.Invoke(handler, [command, cancellationToken])!;

        return await task;
    }

    public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = _serviceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.HandleAsync));
        var task = (Task)method!.Invoke(handler, [command, cancellationToken])!;

        await task;
    }
}
