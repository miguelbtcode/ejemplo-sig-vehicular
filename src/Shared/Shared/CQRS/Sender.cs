using Shared.Contracts.CQRS;

namespace Shared.CQRS;

public class Sender(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    : ISender
{
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher = queryDispatcher;

    public async Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
    {
        return await _commandDispatcher.DispatchAsync(command, cancellationToken);
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
    }

    public async Task<TResponse> SendAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
    {
        return await _queryDispatcher.DispatchAsync(query, cancellationToken);
    }
}
