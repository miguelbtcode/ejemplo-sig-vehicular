using System.Text;
using Identity.Authentication.Middleware;
using Identity.Authentication.Services;
using Identity.Authorization.Handlers;
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
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(
                            typeof(IdentityDbContext).Assembly.FullName
                        );
                        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                    }
                );
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

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services
            .AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));

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
