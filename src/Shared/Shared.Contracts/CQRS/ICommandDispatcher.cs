namespace Shared.Contracts.CQRS;

public interface ICommandDispatcher
{
    Task<TResponse> DispatchAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    );
    Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default);
}
