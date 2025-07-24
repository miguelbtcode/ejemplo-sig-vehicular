using Shared.Exceptions;

namespace Api.Middlewares;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessException(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleGenericException(context, ex);
        }
    }

    private static async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        var response = new ErrorResponse(
            "Validation.Failed",
            "Validation errors occurred",
            ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
        );

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task HandleBusinessException(HttpContext context, BusinessException ex)
    {
        var response = new ErrorResponse(ex.Code, ex.Message);

        var statusCode = ex.Code switch
        {
            var code when code.EndsWith("NotFound") => 404,
            var code when code.EndsWith("AlreadyExists") => 409,
            var code when code.EndsWith("InvalidCredentials") => 401,
            var code when code.EndsWith("InactiveUser") => 403,
            _ => 400,
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task HandleGenericException(HttpContext context, Exception ex)
    {
        var response = new ErrorResponse("Internal.ServerError", "An unexpected error occurred");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(response);
    }
}
