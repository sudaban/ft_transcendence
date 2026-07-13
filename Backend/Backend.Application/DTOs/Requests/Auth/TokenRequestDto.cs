namespace Backend.Application.DTOs.Requests.Auth
{
    public record TokenRequestDto
    (
        string AccessToken,
        string RefreshToken
    );
}
