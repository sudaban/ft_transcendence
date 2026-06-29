using Backend.Domain.Entities;

namespace Backend.Application.Abstractions
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}