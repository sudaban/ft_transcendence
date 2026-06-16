using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public class ChatRoom
{
    public int Id { get; set; }
    
    public bool IsGroup { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
    
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
