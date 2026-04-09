using AuthenticationService.Models;

namespace AuthenticationService.Services;

/// <summary>
/// Interface for session data persistence operations.
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Creates a new session.
    /// </summary>
    Task<Session> CreateAsync(Session session);

    /// <summary>
    /// Gets a session by token.
    /// </summary>
    Task<Session?> GetByTokenAsync(string token);

    /// <summary>
    /// Revokes a session by token.
    /// </summary>
    Task<bool> RevokeByTokenAsync(string token);

    /// <summary>
    /// Revokes all sessions for a user.
    /// </summary>
    Task RevokeAllForUserAsync(Guid userId);
}
