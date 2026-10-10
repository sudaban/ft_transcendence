namespace Backend.Application.Abstractions;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash, string? passwordSalt = null);
    bool NeedsRehash(string passwordHash);
}
