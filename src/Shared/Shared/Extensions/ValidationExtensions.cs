using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.CQRS;
using Shared.Decorators;

namespace Shared.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidationWithAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        // Add validatiors
        services.AddValidatorsFromAssemblies(assemblies);

        // Add decorators
        services.Decorate<ISender, ResultExceptionDecorator>();
        services.Decorate<ISender, ValidationDecorator>();

        return services;
    }
}
