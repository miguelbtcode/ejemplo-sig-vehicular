using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Contracts.CQRS;

namespace Shared.Decorators;

public class LoggingQueryHandlerDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> handler,
    ILogger<LoggingQueryHandlerDecorator<TQuery, TResponse>> logger
) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _handler = handler;
    private readonly ILogger<LoggingQueryHandlerDecorator<TQuery, TResponse>> _logger = logger;

    public async Task<TResponse> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var queryName = typeof(TQuery).Name;

        _logger.LogInformation("Executing query {QueryName}", queryName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _handler.HandleAsync(query, cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Query {QueryName} executed successfully in {ElapsedMs}ms",
                queryName,
                stopwatch.ElapsedMilliseconds
            );

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Query {QueryName} failed after {ElapsedMs}ms",
                queryName,
                stopwatch.ElapsedMilliseconds
            );
            throw;
        }
    }
}
