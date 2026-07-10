using Backend.Application.DTOs.Responses.Users;
using System;
using System.Collections.Generic;

namespace Backend.Application.DTOs.Responses.ChatRooms
{
    public record ChatRoomDto(
        int Id,
        bool IsGroup,
        DateTime CreatedAt,
        IReadOnlyCollection<UserDto> Members
    );
}
