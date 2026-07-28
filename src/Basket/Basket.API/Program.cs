using Basket.API.Data;
using Basket.API.Models;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CONEXIONES
// ======================================================

var databaseConnection =
    builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "No se encontró la conexión ConnectionStrings__Database."
    );

var redisConnection =
    builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException(
        "No se encontró la conexión ConnectionStrings__Redis."
    );

var redisOptions = CreateRedisOptions(redisConnection);

// ======================================================
// CARTER, MEDIATR Y VALIDACIONES
// ======================================================

builder.Services.AddCarter();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(
    typeof(Program).Assembly
);

// ======================================================
// POSTGRESQL CON MARTEN
// ======================================================

builder.Services.AddMarten(opts =>
{
    opts.Connection(databaseConnection);

    opts.Schema
        .For<ShoppingCart>()
        .Identity(x => x.UserName);
})
.UseLightweightSessions();

// ======================================================
// REPOSITORIO Y PATRÓN DECORADOR
// ======================================================

builder.Services.AddScoped<
    IBasketRepository,
    BasketRepository
>();

builder.Services.Decorate<
    IBasketRepository,
    CacheBasketRepository
>();

// ======================================================
// REDIS
// ======================================================

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = redisOptions;
});

// ======================================================
// CORS
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins =
            builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>()
            ?? [];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ======================================================
// MANEJO DE ERRORES
// ======================================================

builder.Services.AddExceptionHandler<
    CustomExceptionHandler
>();

builder.Services.AddProblemDetails();

// Este health check comprueba PostgreSQL.
// Redis tendrá un endpoint independiente.
builder.Services
    .AddHealthChecks()
    .AddNpgSql(databaseConnection);

// ======================================================
// CONSTRUIR LA APLICACIÓN
// ======================================================

var app = builder.Build();

app.UseCors("Frontend");

app.MapCarter();

app.UseExceptionHandler(options => { });

// ======================================================
// HEALTH CHECK GENERAL
// ======================================================

app.UseHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter =
            UIResponseWriter.WriteHealthCheckUIResponse
    }
);

// Este endpoint solamente comprueba que Basket.API inició.
// Este será el health check utilizado por Render.
app.MapGet("/health/live", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "Basket.API"
    });
});

// Este endpoint prueba directamente la conexión con Upstash.
app.MapGet("/health/redis", async () =>
{
    try
    {
        using var connection =
            await ConnectionMultiplexer.ConnectAsync(
                redisOptions
            );

        var database = connection.GetDatabase();

        var latency = await database.PingAsync();

        return Results.Ok(new
        {
            status = "Healthy",
            service = "Redis",
            latencyMilliseconds =
                latency.TotalMilliseconds
        });
    }
    catch (Exception exception)
    {
        return Results.Problem(
            title: "No se pudo conectar con Redis",
            detail: exception.Message,
            statusCode: StatusCodes
                .Status503ServiceUnavailable
        );
    }
});

app.Run();

// ======================================================
// CONVERTIR LA CONEXIÓN DE REDIS
// ======================================================

static ConfigurationOptions CreateRedisOptions(
    string connectionString
)
{
    var normalizedConnection =
        connectionString.Trim();

    // Conexión de Upstash:
    // rediss://default:password@host:6379
    if (
        normalizedConnection.StartsWith(
            "rediss://",
            StringComparison.OrdinalIgnoreCase
        )
        ||
        normalizedConnection.StartsWith(
            "redis://",
            StringComparison.OrdinalIgnoreCase
        )
    )
    {
        var uri = new Uri(normalizedConnection);

        var credentials =
            uri.UserInfo.Split(':', 2);

        if (credentials.Length != 2)
        {
            throw new InvalidOperationException(
                "La URL de Redis no contiene usuario y contraseña."
            );
        }

        var user =
            Uri.UnescapeDataString(credentials[0]);

        var password =
            Uri.UnescapeDataString(credentials[1]);

        var useSsl = uri.Scheme.Equals(
            "rediss",
            StringComparison.OrdinalIgnoreCase
        );

        var port = uri.IsDefaultPort
            ? 6379
            : uri.Port;

        var options = new ConfigurationOptions
        {
            User = user,
            Password = password,
            Ssl = useSsl,
            SslHost = useSsl
                ? uri.Host
                : null,
            AbortOnConnectFail = false,
            ConnectTimeout = 15000,
            SyncTimeout = 15000,
            ConnectRetry = 3
        };

        options.EndPoints.Add(
            uri.Host,
            port
        );

        return options;
    }

    // Conexión local:
    // redis:6379
    var localOptions =
        ConfigurationOptions.Parse(
            normalizedConnection
        );

    localOptions.AbortOnConnectFail = false;
    localOptions.ConnectTimeout = 15000;
    localOptions.SyncTimeout = 15000;
    localOptions.ConnectRetry = 3;

    return localOptions;
}
