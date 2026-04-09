using AuthenticationService.Core.Interfaces;
using BCrypt.Net;

namespace AuthenticationService.Infrastructure.Services;

/// <summary>
/// BCrypt implementation of password hashing.
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (SaltParseException)
        {
            return false;
        }
    }
}
