namespace Shared.Contracts.CQRS;

public interface IQueryDispatcher
{
    Task<TResponse> DispatchAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    );
}
