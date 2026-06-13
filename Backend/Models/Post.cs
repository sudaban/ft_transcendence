using System;
using System.Collections.Generic;

namespace Backend.Models;

public class Post
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public string ImageUrl { get; set; } = null!;
    
    public string? Caption { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
    
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
