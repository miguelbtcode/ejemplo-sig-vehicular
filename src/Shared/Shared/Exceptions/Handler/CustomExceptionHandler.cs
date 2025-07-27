using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Shared.Exceptions.Handler;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "Unhandled exception");

        (string Detail, string Title, int StatusCode) details = exception switch
        {
            ValidationException => (
                FormatValidationDetail((ValidationException)exception),
                exception.GetType().Name,
                StatusCodes.Status400BadRequest
            ),
            BusinessException businessEx => (
                businessEx.Message,
                exception.GetType().Name,
                GetBusinessExceptionStatusCode(businessEx.Code)
            ),
            _ => (
                "An unexpected error occurred",
                exception.GetType().Name,
                StatusCodes.Status500InternalServerError
            ),
        };

        var problemDetails = new ProblemDetails
        {
            Title = details.Title,
            Detail = details.Detail,
            Status = details.StatusCode,
            Instance = context.Request.Path,
        };

        problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions.Add("ValidationErrors", validationException.Errors);
        }

        context.Response.StatusCode = details.StatusCode;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static string FormatValidationDetail(ValidationException validationException)
    {
        var errors = validationException
            .Errors.Select(error =>
                $" -- {error.PropertyName}: {error.ErrorMessage}. Severity: {error.Severity}"
            )
            .ToArray();

        return $"Validation failed: \n{string.Join("\n", errors)}";
    }

    private static int GetBusinessExceptionStatusCode(string code) =>
        code switch
        {
            var c when c.EndsWith("NotFound") => StatusCodes.Status404NotFound,
            var c when c.EndsWith("AlreadyExists") => StatusCodes.Status409Conflict,
            var c when c.EndsWith("InvalidCredentials") => StatusCodes.Status401Unauthorized,
            var c when c.EndsWith("InactiveUser") => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest,
        };
}
