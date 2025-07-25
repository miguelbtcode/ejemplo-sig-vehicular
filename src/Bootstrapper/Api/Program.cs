using Api.Middlewares;
using Carter;
using FluentValidation;
using Identity;
using Identity.Users.Features.CreateUser;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    static (context, config) => config.ReadFrom.Configuration(context.Configuration)
);

var identityAssembly = typeof(IdentityModule).Assembly;

builder.Services.AddCarterWithAssemblies(identityAssembly);
builder.Services.AddCQRS(identityAssembly);
builder.Services.AddDDD(identityAssembly);

builder.Services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();

builder.Services.AddValidationWithAssemblies(identityAssembly);

// Autorización
builder.Services.AddAuthorization();

builder.Services.AddIdentityModule(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(static c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "SigVehicular API",
            Version = "v1",
            Description = "API para gestión vehicular",
        }
    );

    // Configuración para JWT en Swagger
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );

    // Ordering tags alphabetically
    c.OrderActionsBy(apiDesc =>
        $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}"
    );
});

var app = builder.Build();

// Configurar pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseSwagger();
app.UseSwaggerUI(static c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SigVehicular API V1");
    c.RoutePrefix = string.Empty; // Para que Swagger esté en la raíz
});

app.MapCarter();

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseIdentityModule();

app.UseAuthentication();

app.UseAuthorization();

app.Run();
