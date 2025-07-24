using Api.Middlewares;
using Carter;
using FluentValidation;
using Identity;
using Serilog;
using Shared.CQRS;
using Shared.DDD;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

var identityAssembly = typeof(IdentityModule).Assembly;

builder.Services.AddCarterWithAssemblies(identityAssembly);

builder.Services.AddCQRS(identityAssembly);

builder.Services.AddDDD(identityAssembly);

// Validaciones
builder.Services.AddValidatorsFromAssembly(identityAssembly);

// Autorización
builder.Services.AddAuthorization();

builder.Services.AddIdentityModule(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SigVehicular API", Version = "v1" }
    );

    // Configuración para JWT en Swagger
    c.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    c.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
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
app.UseSwaggerUI(c =>
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
