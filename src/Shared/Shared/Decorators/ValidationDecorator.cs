using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.CQRS;

namespace Shared.Decorators;

public class ValidationDecorator : ISender
{
    private readonly ISender _inner;
    private readonly IServiceProvider _serviceProvider;

    public ValidationDecorator(ISender inner, IServiceProvider serviceProvider)
    {
        _inner = inner;
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
    {
        await ValidateAsync(command);
        return await _inner.SendAsync(command, cancellationToken);
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(command);
        await _inner.SendAsync(command, cancellationToken);
    }

    public async Task<TResponse> SendAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
    {
        await ValidateAsync(query);
        return await _inner.SendAsync(query, cancellationToken);
    }

    private async Task ValidateAsync<T>(T request)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        if (validator == null)
            return;

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
