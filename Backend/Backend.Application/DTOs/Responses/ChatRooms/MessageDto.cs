using Backend.Application.DTOs.Responses.Users;
using System;

namespace Backend.Application.DTOs.Responses.ChatRooms;

public class MessageDto
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }

    public UserDto Sender { get; set; } = null!;
}
