using AuthenticationService.Models;

namespace AuthenticationService.Services;

/// <summary>
/// Interface for user data persistence operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    Task<bool> ExistsAsync(string email);
}
