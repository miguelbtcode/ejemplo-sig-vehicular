using System.Text;
using Identity.Authentication.Middleware;
using Identity.Authentication.Services;
using Identity.Authorization.Handlers;
using Identity.Authorization.Requirements;
using Identity.Data.Seed;
using Identity.Permissions.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Data;
using Shared.Data.Interceptors;

namespace Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Database
        var connectionString = configuration.GetConnectionString("Database");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<IdentityDbContext>(
            (sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString);
            }
        );

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IDataSeeder, IdentityDataSeeder>();

        // JWT Authentication
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                    ),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        // Authorization
        services.AddAuthorization(options =>
        {
            // Crear políticas dinámicamente para permisos
            var modules = new[] { "Catalog", "Basket", "Ordering", "Identity" };
            var permissions = new[] { "Crear", "Leer", "Actualizar", "Eliminar", "Administrar" };

            foreach (var module in modules)
            {
                foreach (var permission in permissions)
                {
                    var policyName = $"Permission.{module}.{permission}";
                    options.AddPolicy(
                        policyName,
                        policy =>
                            policy.Requirements.Add(new PermissionRequirement(module, permission))
                    );
                }
            }
        });

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    public static IApplicationBuilder UseIdentityModule(this IApplicationBuilder app)
    {
        // Migrate database
        app.UseMigration<IdentityDbContext>();

        // Use JWT middleware
        app.UseMiddleware<JwtMiddleware>();

        return app;
    }
}
