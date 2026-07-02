using System.Collections.Generic;

namespace Backend.Application.DTOs.Requests.Messages
{
    public record CreateChatRoomRequestDto
    (
        string? Name,
        List<int> ParticipantIds
    );
}
