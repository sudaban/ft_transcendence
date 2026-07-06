using System;

namespace Backend.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    // Bildirimi tetikleyen kullanıcı (Örn: "Ahmet gönderini beğendi" -> Actor=Ahmet)
    public int ActorId { get; set; }
    public User Actor { get; set; } = null!;
    
    public NotificationType Type { get; set; }
    
    // İlgili kaynağın ID'si (PostId, CommentId, FollowId vb.)
    public int? ReferenceId { get; set; }
    
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    Like,
    Comment,
    Follow,
    Message
}
