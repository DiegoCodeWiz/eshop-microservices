using Catalog.API.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddCarter();

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseCors("Frontend");

//utilizamos carter como parte de minimal api
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Catalog.API" }));
app.MapCarter();
app.UseExceptionHandler(options => { });

app.Run();
