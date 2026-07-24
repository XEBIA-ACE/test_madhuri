```csharp
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));
    
builder.Services.AddScoped<ITokenService, TokenService>();

// Existing services
builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection(TokenSettings.SectionName));

// Existing configurations continue here...
// Add health checks, CORS, swagger, etc.
```