using FluentValidation.Results;

namespace Shared.Exceptions;

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Validation failed")
    {
        Errors = failures;
    }
}
