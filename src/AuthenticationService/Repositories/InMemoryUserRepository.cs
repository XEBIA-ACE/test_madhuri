using AuthenticationService.Models.Entities;
using BCrypt.Net;

namespace AuthenticationService.Repositories;

/// <summary>
/// In-memory implementation of the user repository for development and testing.
/// TODO: Replace with a database-backed implementation for production.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users;

    public InMemoryUserRepository()
    {
        // Seed with test users for development
        _users = new List<User>
        {
            new User
            {
                Id = Guid.Parse("6ac6b43c-29aa-4b94-abde-18897563e8e1"),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    /// <inheritdoc />
    public Task<User?> GetByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public Task<User?> GetByIdAsync(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public bool VerifyPassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
}
