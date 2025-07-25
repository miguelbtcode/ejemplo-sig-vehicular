using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.DDD;
using Shared.DDD.Dispatchers;

namespace Shared.Extensions;

public static class DddExtensions
{
    public static IServiceCollection AddDDD(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        // Registrar Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Registrar Domain Event Handlers
        services.Scan(scan =>
            scan.FromAssemblies(assemblies)
                .AddClasses(c =>
                    c.Where(type =>
                        type.GetInterfaces()
                            .Any(i =>
                                i.IsGenericType
                                && (i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                            )
                    )
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return services;
    }
}
