using System;

namespace Backend.Models;

public class ChatRoomMember
{
    public int ChatRoomId { get; set; }
    
    public int UserId { get; set; }
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public ChatRoom ChatRoom { get; set; } = null!;
    
    public User User { get; set; } = null!;
}
