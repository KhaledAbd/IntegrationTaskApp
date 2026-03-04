using GitHubIntegrationApi.Middleware;
using GitHubIntegrationApi.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "v1";
    config.Title = "GitHub Integration API";
    config.Version = "v1";
});

builder.Services.AddScoped<IGitHubIntegrationRepository, GitHubIntegrationRepository>();
builder.Services.AddHealthChecks();

// Add CORS Policy
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost4200", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowLocalhost4200");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting GitHub Integration API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
