namespace AuthenticationService.Core.Exceptions;

/// <summary>
/// Base exception for authentication-related errors.
/// </summary>
public class AuthenticationException : Exception
{
    public int StatusCode { get; }

    public AuthenticationException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }

    public AuthenticationException(string message, Exception innerException, int statusCode = 400) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Exception thrown when user registration fails.
/// </summary>
public class RegistrationException : AuthenticationException
{
    public RegistrationException(string message) : base(message, 400) { }
}

/// <summary>
/// Exception thrown when login credentials are invalid.
/// </summary>
public class InvalidCredentialsException : AuthenticationException
{
    public InvalidCredentialsException() : base("Invalid email or password", 401) { }
}

/// <summary>
/// Exception thrown when a user is not found.
/// </summary>
public class UserNotFoundException : AuthenticationException
{
    public UserNotFoundException() : base("User not found", 404) { }
}

/// <summary>
/// Exception thrown when a token is invalid or expired.
/// </summary>
public class InvalidTokenException : AuthenticationException
{
    public InvalidTokenException(string message = "Invalid or expired token") : base(message, 401) { }
}

/// <summary>
/// Exception thrown when a user already exists.
/// </summary>
public class UserAlreadyExistsException : AuthenticationException
{
    public UserAlreadyExistsException(string email) : base($"User with email '{email}' already exists", 409) { }
}
