using System.Security.Cryptography;
using AuthenticationService.Configuration;
using AuthenticationService.Models.Entities;
using AuthenticationService.Stores;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Services;

/// <summary>
/// Manages token lifecycle: issuance, validation, and invalidation.
/// </summary>
public sealed class TokenManager : ITokenManager
{
    private readonly ITokenStore _tokenStore;
    private readonly TokenSettings _settings;
    private readonly ILogger<TokenManager> _logger;

    public TokenManager(
        ITokenStore tokenStore,
        IOptions<TokenSettings> settings,
        ILogger<TokenManager> logger)
    {
        _tokenStore = tokenStore;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TokenInfo> IssueTokenAsync(string username, CancellationToken cancellationToken = default)
    {
        var token = GenerateToken();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_settings.TokenExpirationMinutes);

        var tokenInfo = new TokenInfo
        {
            Token = token,
            Username = username,
            IssuedAt = now,
            ExpiresAt = expiresAt,
            IsInvalidated = false
        };

        await _tokenStore.StoreAsync(tokenInfo, cancellationToken);

        _logger.LogInformation(
            "Issued token for user: {Username}, expires at: {ExpiresAt}",
            username, expiresAt);

        return tokenInfo;
    }

    /// <inheritdoc />
    public async Task<TokenInfo?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var tokenInfo = await _tokenStore.GetAsync(token, cancellationToken);

        if (tokenInfo == null)
        {
            _logger.LogDebug("Token not found during validation");
            return null;
        }

        if (!tokenInfo.IsValid)
        {
            _logger.LogDebug(
                "Token validation failed for user: {Username}, IsInvalidated: {IsInvalidated}, ExpiresAt: {ExpiresAt}",
                tokenInfo.Username, tokenInfo.IsInvalidated, tokenInfo.ExpiresAt);
            return null;
        }

        _logger.LogDebug("Token validated successfully for user: {Username}", tokenInfo.Username);
        return tokenInfo;
    }

    /// <inheritdoc />
    public async Task<bool> InvalidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var result = await _tokenStore.InvalidateAsync(token, cancellationToken);

        if (result)
        {
            _logger.LogInformation("Token invalidated successfully");
        }
        else
        {
            _logger.LogDebug("Token not found for invalidation");
        }

        return result;
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// TODO: Consider using JWT format for stateless validation if needed.
    /// </summary>
    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
