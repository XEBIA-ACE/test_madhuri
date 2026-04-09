using AuthenticationService.Core.Configuration;
using AuthenticationService.Core.Interfaces;
using AuthenticationService.Infrastructure.Data;
using AuthenticationService.Infrastructure.Repositories;
using AuthenticationService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationService.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<ServiceSettings>(configuration.GetSection(ServiceSettings.SectionName));

        // Configure database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            // Use in-memory database for development/testing
            services.AddDbContext<AuthDbContext>(options =>
                options.UseInMemoryDatabase("AuthenticationServiceDb"));
        }
        else
        {
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        // Register services
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationServiceImpl>();

        return services;
    }
}
