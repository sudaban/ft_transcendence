using System;

namespace Backend.Application.DTOs.Responses.ChatRooms
{
    public record ChatRoomDto(
        int Id,
        bool IsGroup,
        DateTime CreatedAt
    );
}
