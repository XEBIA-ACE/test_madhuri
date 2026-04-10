using AuthenticationService.Configuration;
using AuthenticationService.Middleware;
using AuthenticationService.Services;
using AuthenticationService.Stores;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection(TokenSettings.SectionName));

// Add stores (singleton for in-memory implementations)
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
builder.Services.AddSingleton<ITokenStore, InMemoryTokenStore>();

// Add services
builder.Services.AddScoped<IUserCredentialValidator, UserCredentialValidator>();
builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddSingleton<IAuthenticationMetrics, AuthenticationMetrics>();

// Add background services
builder.Services.AddHostedService<TokenCleanupService>();

// Add controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Authentication Service",
        Version = "v1",
        Description = "Secure authentication, token management, and session handling for platform users and services."
    });
});

// Configure forwarded headers for reverse proxy scenarios
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware pipeline
app.UseForwardedHeaders();

// Exception handling (first in pipeline)
app.UseExceptionHandling();

// Request logging
app.UseRequestLogging();

// HTTPS enforcement
app.UseHttpsEnforcement();

// HTTPS redirection (for development)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS
app.UseCors();

// Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Authentication Service v1");
        options.RoutePrefix = "swagger";
    });
}

// Health checks
app.MapHealthChecks("/health");

// Map controllers
app.MapControllers();

// Metrics endpoint
app.MapGet("/metrics", (IAuthenticationMetrics metrics) => metrics.GetSnapshot())
    .WithName("GetMetrics")
    .WithOpenApi();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
