using System.Collections.Concurrent;
using AuthenticationService.Models;

namespace AuthenticationService.Services;

/// <summary>
/// In-memory implementation of user repository for development/testing.
/// TODO: Replace with actual database implementation (e.g., Entity Framework, Dapper).
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly ConcurrentDictionary<string, Guid> _emailIndex = new(StringComparer.OrdinalIgnoreCase);

    public Task<User?> GetByIdAsync(Guid id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        if (_emailIndex.TryGetValue(email, out var userId))
        {
            _users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }
        return Task.FromResult<User?>(null);
    }

    public Task<User> CreateAsync(User user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        if (!_emailIndex.TryAdd(user.Email, user.Id))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        _users[user.Id] = user;
        return Task.FromResult(user);
    }

    public Task<bool> ExistsAsync(string email)
    {
        return Task.FromResult(_emailIndex.ContainsKey(email));
    }
}
