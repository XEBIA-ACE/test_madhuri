using AuthenticationService.Models.Entities;

namespace AuthenticationService.Repositories;

/// <summary>
/// Repository interface for user data access.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Verifies if the provided password matches the user's stored password hash.
    /// </summary>
    /// <param name="user">The user to verify against.</param>
    /// <param name="password">The plain text password to verify.</param>
    /// <returns>True if the password is valid, false otherwise.</returns>
    bool VerifyPassword(User user, string password);
}
