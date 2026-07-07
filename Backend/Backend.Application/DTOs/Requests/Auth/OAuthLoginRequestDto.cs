namespace Backend.Application.DTOs.Requests.Auth
{
    public record OAuthLoginRequestDto
    (
        string Provider,
        string Code,
        string RedirectUri
    );
}
