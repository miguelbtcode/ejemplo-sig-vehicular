using Identity.Authentication.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Extensions;

public static class ApiConfigurationExtensions
{
    public static IServiceCollection AddIdentityApiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // JWT Configuration
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // CORS if needed
        services.AddCors(options =>
        {
            options.AddPolicy(
                "IdentityPolicy",
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        return services;
    }

    public static IApplicationBuilder UseIdentityApiConfiguration(this IApplicationBuilder app)
    {
        app.UseCors("IdentityPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
