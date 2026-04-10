using AuthenticationService.Models.Entities;

namespace AuthenticationService.Stores;

/// <summary>
/// Interface for user data store operations.
/// </summary>
public interface IUserStore
{
    /// <summary>
    /// Gets user credentials by username.
    /// </summary>
    /// <param name="username">The username to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User credentials if found, null otherwise.</returns>
    Task<UserCredentials?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists by username.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if user exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);
}
