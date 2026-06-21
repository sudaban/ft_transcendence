namespace Backend.Application.DTOs.Requests.Users
{
    public record UpdateProfileRequestDto
    (
    string? FullName,
    string? Bio,
    string? ProfilePictureUrl
    );
}
