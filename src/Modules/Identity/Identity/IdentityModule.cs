using System.Text;
using Identity.Authentication.Configuration;
using Identity.Authentication.Middleware;
using Identity.Authentication.Services;
using Identity.Authentication.Services.Implementation;
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
        // Add HttpAccessor
        services.AddHttpContextAccessor();

        // Database
        AddDatabase(services, configuration);

        // Authentication Services
        AddAuthenticationServices(services, configuration);

        // Authorization Services
        AddAuthorizationServices(services);

        // JWT Configuration
        AddJwtAuthentication(services, configuration);

        return services;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
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

        services.AddScoped<IDataSeeder, IdentityDataSeeder>();
    }

    private static void AddAuthenticationServices(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Jwt settings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Core authentication services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Permission services
        services.AddScoped<IPermissionService, PermissionService>();
    }

    private static void AddAuthorizationServices(IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services
            .AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));
    }

    private static void AddJwtAuthentication(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
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

                // Handle token from query string for SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                };
            });
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
