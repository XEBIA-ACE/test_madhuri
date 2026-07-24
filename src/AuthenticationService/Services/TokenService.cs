```csharp
using System.Collections.Concurrent;
using System.Security.Cryptography;
using AuthenticationService.Models.Entities;

namespace AuthenticationService.Services;

/// <summary>
/// Implements the ITokenService interface.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(User user)
    {
        var token = GenerateSecureToken();
        var expirationTime = DateTime.UtcNow.AddMinutes(60); // 1 hour expiration

        var tokenInfo = new TokenInfo
        {
            Token = token,
            Username = user.Username,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expirationTime,
            IsInvalidated = false
        };

        // Simulate saving to a token store
        await Task.CompletedTask;

        return (token, expirationTime);
    }

    public async Task<(bool IsValid, Guid? UserId, string? Username, DateTime? ExpiresAt, string? ErrorMessage)> ValidateTokenAsync(string token)
    {
        // Simulate validation
        await Task.CompletedTask;
        return (true, Guid.NewGuid(), "demo_user", DateTime.UtcNow.AddMinutes(30), null);
    }

    public async Task<bool> InvalidateTokenAsync(string token)
    {
        // Simulate invalidation logic
        await Task.CompletedTask;
        return true;
    }

    public string? ExtractTokenId(string token)
    {
        // Placeholder for real extraction logic
        return null;
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
```