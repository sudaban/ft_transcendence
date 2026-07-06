namespace Backend.Application.DTOs.Requests.Messages
{
    public record SendMessageRequestDto
    (
        int ChatRoomId,
        string Content
    );
}
