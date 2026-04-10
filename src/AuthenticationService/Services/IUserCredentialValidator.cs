namespace AuthenticationService.Services;

/// <summary>
/// Result of credential validation.
/// </summary>
public sealed class CredentialValidationResult
{
    /// <summary>
    /// Whether the credentials are valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The username if validation succeeded.
    /// </summary>
    public string? Username { get; init; }

    public static CredentialValidationResult Success(string username) => new()
    {
        IsValid = true,
        Username = username
    };

    public static CredentialValidationResult Failure(string errorMessage) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Interface for validating user credentials.
/// </summary>
public interface IUserCredentialValidator
{
    /// <summary>
    /// Validates user credentials against the user data store.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result indicating success or failure.</returns>
    Task<CredentialValidationResult> ValidateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
