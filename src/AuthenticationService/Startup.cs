```csharp
// Add JWT related configurations.
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.Configure<JwtSettings>(Configuration.GetSection(JwtSettings.SectionName));
    // ...other service configurations
}
```