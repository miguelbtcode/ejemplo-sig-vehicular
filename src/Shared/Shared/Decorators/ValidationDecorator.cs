using FluentValidation;
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
        await ValidateAsync(command, cancellationToken);
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

    private async Task ValidateAsync<T>(T request, CancellationToken cancellationToken = default)
        where T : class
    {
        Type commandType = request.GetType();

        Type validatorType = typeof(IValidator<>).MakeGenericType(commandType);

        object? validator = _serviceProvider.GetService(validatorType);

        if (validator is null)
        {
            return;
        }

        var validationContext = (IValidationContext)
            Activator.CreateInstance(
                typeof(ValidationContext<>).MakeGenericType(commandType),
                request
            )!;

        var validationResult = await ((IValidator)validator).ValidateAsync(
            validationContext,
            cancellationToken
        );

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
