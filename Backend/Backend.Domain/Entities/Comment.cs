using System;

namespace Backend.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    
    public int PostId { get; set; }
    
    public int UserId { get; set; }
    
    public string Content { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Post Post { get; set; } = null!;
    
    public User User { get; set; } = null!;
}
