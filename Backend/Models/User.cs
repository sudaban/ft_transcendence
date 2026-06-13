using System;
using System.Collections.Generic;

namespace Backend.Models;

public class User
{
    public int Id { get; set; }
    
    public string Username { get; set; } = null!;
    
    public string Email { get; set; } = null!;
    
    public string PasswordHash { get; set; } = null!;
    
    public string PasswordSalt { get; set; } = null!;
    
    public string? ProfilePictureUrl { get; set; }
    
    public string? Bio { get; set; }
    
    public bool IsTwoFactorEnabled { get; set; } = false;
    
    public string? TwoFactorSecret { get; set; }
    
    public bool IsTosAccepted { get; set; } = false;
    
    public DateTime? TosAcceptedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Follow> FollowedBy { get; set; } = new List<Follow>();
    
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
    
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    
    public ICollection<ChatRoomMember> ChatRoomMemberships { get; set; } = new List<ChatRoomMember>();
    
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
}
