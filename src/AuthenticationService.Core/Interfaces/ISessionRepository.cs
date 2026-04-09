using AuthenticationService.Core.Entities;

namespace AuthenticationService.Core.Interfaces;

/// <summary>
/// Repository interface for session data access.
/// </summary>
public interface ISessionRepository
{
    Task<Session?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Session?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<Session>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Session> CreateAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default);
    Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
