using System.Security.Cryptography;
using System.Text;
using Backend.Application.Abstractions;

namespace Backend.Infrastructure;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 12);
    }

    public bool VerifyPassword(string password, string passwordHash, string? passwordSalt = null)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return false;

        if (passwordHash.StartsWith("$2a$") || passwordHash.StartsWith("$2b$") || passwordHash.StartsWith("$2y$"))
        {
            try
            {
                return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        if (!string.IsNullOrEmpty(passwordSalt))
        {
            try
            {
                using var hmac = new HMACSHA512(Convert.FromBase64String(passwordSalt));
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                var storedHashBytes = Convert.FromBase64String(passwordHash);
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public bool NeedsRehash(string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
            return true;

        if (!passwordHash.StartsWith("$2a$") && !passwordHash.StartsWith("$2b$") && !passwordHash.StartsWith("$2y$"))
            return true;

        return false;
    }
}
