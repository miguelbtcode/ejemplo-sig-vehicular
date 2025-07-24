using Shared.Contracts.CQRS;
using Shared.Exceptions;
using Shared.Results;

namespace Shared.Decorators;

public class ResultExceptionDecorator : ISender
{
    private readonly ISender _inner;

    public ResultExceptionDecorator(ISender inner) => _inner = inner;

    public async Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _inner.SendAsync(command, cancellationToken);
        ThrowIfResultFailure(response);
        return response;
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        await _inner.SendAsync(command, cancellationToken);
    }

    public async Task<TResponse> SendAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _inner.SendAsync(query, cancellationToken);
        ThrowIfResultFailure(response);
        return response;
    }

    private static void ThrowIfResultFailure<T>(T response)
    {
        if (response is Result result && result.IsFailure)
        {
            throw new BusinessException(result.Error.Code, result.Error.Name);
        }
    }
}
