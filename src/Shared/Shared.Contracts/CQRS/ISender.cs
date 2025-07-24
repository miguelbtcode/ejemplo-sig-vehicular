namespace Shared.Contracts.CQRS;

public interface ISender
{
    Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    );
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);
    Task<TResponse> SendAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    );
}
