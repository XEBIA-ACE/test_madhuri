using AuthenticationService.Core.Entities;
using AuthenticationService.Core.Interfaces;
using AuthenticationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for session data access.
/// </summary>
public class SessionRepository : ISessionRepository
{
    private readonly AuthDbContext _context;

    public SessionRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);
    }

    public async Task<Session?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<Session> CreateAsync(Session session, CancellationToken cancellationToken = default)
    {
        session.Id = Guid.NewGuid();
        session.CreatedAt = DateTime.UtcNow;

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return session;
    }

    public async Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);

        return session;
    }

    public async Task RevokeAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _context.Sessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
