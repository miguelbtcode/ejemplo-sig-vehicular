namespace Shared.Models;

public record ErrorResponse(string Code, string Message, object? Details = null);
