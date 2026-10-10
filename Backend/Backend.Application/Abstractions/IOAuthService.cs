using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public record OAuthUserInfo(string Email, string Username);

public interface IOAuthService
{
    Task<OAuthUserInfo> GetOAuthUserInfoAsync(string provider, string code, string redirectUri);
}
