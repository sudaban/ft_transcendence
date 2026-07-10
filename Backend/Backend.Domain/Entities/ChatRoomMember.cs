using System;

namespace Backend.Domain.Entities;

public class ChatRoomMember
{
    public int ChatRoomId { get; set; }
    
    public int UserId { get; set; }
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsHidden { get; set; } = false;

    public DateTime? ClearedAt { get; set; }
    
    public ChatRoom ChatRoom { get; set; } = null!;
    
    public User User { get; set; } = null!;
}
