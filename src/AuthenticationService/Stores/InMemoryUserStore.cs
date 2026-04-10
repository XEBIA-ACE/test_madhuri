using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using AuthenticationService.Models.Entities;

namespace AuthenticationService.Stores;

/// <summary>
/// In-memory implementation of user store for development/testing.
/// TODO: Replace with persistent data store (e.g., SQL Server, PostgreSQL) for production.
/// </summary>
public sealed class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<string, UserCredentials> _users = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<InMemoryUserStore> _logger;

    public InMemoryUserStore(ILogger<InMemoryUserStore> logger)
    {
        _logger = logger;
        SeedDefaultUsers();
    }

    /// <inheritdoc />
    public Task<UserCredentials?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Looking up user: {Username}", username);
        
        _users.TryGetValue(username, out var user);
        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.ContainsKey(username));
    }

    /// <summary>
    /// Seeds default users for development/testing purposes.
    /// </summary>
    private void SeedDefaultUsers()
    {
        // Add a test user for development
        // TODO: Remove or disable in production
        var testUser = new UserCredentials
        {
            UserId = "user-001",
            Username = "testuser",
            PasswordHash = HashPassword("testpassword"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var adminUser = new UserCredentials
        {
            UserId = "user-002",
            Username = "admin",
            PasswordHash = HashPassword("adminpassword"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _users.TryAdd(testUser.Username, testUser);
        _users.TryAdd(adminUser.Username, adminUser);

        _logger.LogInformation("Seeded {Count} default users for development", _users.Count);
    }

    /// <summary>
    /// Hashes a password using SHA256.
    /// TODO: Use a proper password hashing algorithm (e.g., bcrypt, Argon2) for production.
    /// </summary>
    internal static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
