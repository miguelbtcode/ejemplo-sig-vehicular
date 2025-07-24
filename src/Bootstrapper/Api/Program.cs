using Carter;
using Identity;
using Serilog;
using Shared.CQRS;
using Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

var identityAssembly = typeof(IdentityModule).Assembly;

builder.Services.AddCarterWithAssemblies(identityAssembly);

builder.Services.AddCQRS(identityAssembly);

builder.Services.AddAuthorization();

builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

app.MapCarter();

app.UseSerilogRequestLogging();

app.UseIdentityModule();

app.UseAuthorization();

app.Run();
