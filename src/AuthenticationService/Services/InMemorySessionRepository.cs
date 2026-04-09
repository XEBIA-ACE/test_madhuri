using System.Collections.Concurrent;
using AuthenticationService.Models;

namespace AuthenticationService.Services;

/// <summary>
/// In-memory implementation of session repository for development/testing.
/// TODO: Replace with actual database or Redis implementation.
/// </summary>
public class InMemorySessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new();

    public Task<Session> CreateAsync(Session session)
    {
        session.Id = Guid.NewGuid();
        session.CreatedAt = DateTime.UtcNow;
        _sessions[session.Token] = session;
        return Task.FromResult(session);
    }

    public Task<Session?> GetByTokenAsync(string token)
    {
        _sessions.TryGetValue(token, out var session);
        return Task.FromResult(session);
    }

    public Task<bool> RevokeByTokenAsync(string token)
    {
        if (_sessions.TryGetValue(token, out var session))
        {
            session.IsRevoked = true;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task RevokeAllForUserAsync(Guid userId)
    {
        foreach (var session in _sessions.Values.Where(s => s.UserId == userId))
        {
            session.IsRevoked = true;
        }
        return Task.CompletedTask;
    }
}
