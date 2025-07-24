using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Contracts.CQRS;

namespace Shared.Decorators;

public class LoggingCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> handler,
    ILogger<LoggingCommandHandlerDecorator<TCommand, TResponse>> logger
) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _handler = handler;
    private readonly ILogger<LoggingCommandHandlerDecorator<TCommand, TResponse>> _logger = logger;

    public async Task<TResponse> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var commandName = typeof(TCommand).Name;

        _logger.LogInformation("Executing command {CommandName}", commandName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _handler.HandleAsync(command, cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Command {CommandName} executed successfully in {ElapsedMs}ms",
                commandName,
                stopwatch.ElapsedMilliseconds
            );

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Command {CommandName} failed after {ElapsedMs}ms",
                commandName,
                stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }
}
