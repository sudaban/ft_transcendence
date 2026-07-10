using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    
    public int ChatRoomId { get; set; }
    
    public int SenderId { get; set; }
    
    public string Content { get; set; } = null!;
    
    public bool IsRead { get; set; } = false;
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    public ChatRoom ChatRoom { get; set; } = null!;
    
    public User Sender { get; set; } = null!;
    
    public ICollection<DeletedMessage> DeletedByUsers { get; set; } = new List<DeletedMessage>();
}
