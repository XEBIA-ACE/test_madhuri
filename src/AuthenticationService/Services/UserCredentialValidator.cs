using AuthenticationService.Stores;

namespace AuthenticationService.Services;

/// <summary>
/// Validates user credentials against the user data store.
/// </summary>
public sealed class UserCredentialValidator : IUserCredentialValidator
{
    private readonly IUserStore _userStore;
    private readonly ILogger<UserCredentialValidator> _logger;

    public UserCredentialValidator(IUserStore userStore, ILogger<UserCredentialValidator> logger)
    {
        _userStore = userStore;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CredentialValidationResult> ValidateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogDebug("Validation failed: empty username");
            return CredentialValidationResult.Failure("Username is required");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogDebug("Validation failed: empty password");
            return CredentialValidationResult.Failure("Password is required");
        }

        var user = await _userStore.GetByUsernameAsync(username, cancellationToken);

        if (user == null)
        {
            _logger.LogDebug("Validation failed: user not found: {Username}", username);
            // Use generic message to prevent username enumeration
            return CredentialValidationResult.Failure("Invalid username or password");
        }

        if (!user.IsActive)
        {
            _logger.LogDebug("Validation failed: user account is inactive: {Username}", username);
            return CredentialValidationResult.Failure("Account is inactive");
        }

        var passwordHash = InMemoryUserStore.HashPassword(password);
        if (!string.Equals(user.PasswordHash, passwordHash, StringComparison.Ordinal))
        {
            _logger.LogDebug("Validation failed: incorrect password for user: {Username}", username);
            // Use generic message to prevent password enumeration
            return CredentialValidationResult.Failure("Invalid username or password");
        }

        _logger.LogInformation("Credentials validated successfully for user: {Username}", username);
        return CredentialValidationResult.Success(username);
    }
}
