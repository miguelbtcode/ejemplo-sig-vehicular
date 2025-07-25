using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.CQRS;
using Shared.CQRS;
using Shared.CQRS.Dispatchers;
using Shared.Decorators;

namespace Shared.Extensions;

public static class CqrsExtensions
{
    public static IServiceCollection AddCQRS(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        // Registrar dispatchers
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ISender, Sender>();

        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(c =>
                    c.Where(type =>
                        type.GetInterfaces()
                            .Any(i =>
                                i.IsGenericType
                                && (
                                    i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
                                    || i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                                    || i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                                )
                            )
                    )
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // Aplicar decoradores automáticamente
        ApplyDecorators(services);

        return services;
    }

    private static void ApplyDecorators(IServiceCollection services)
    {
        // Decorar Command Handlers con response
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingCommandHandlerDecorator<,>));

        // Decorar Query Handlers
        services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingQueryHandlerDecorator<,>));

        // Si necesitas decorar Command Handlers sin response:
        // services.Decorate(typeof(ICommandHandler<>), typeof(LoggingCommandHandlerDecorator<>));
    }
}
