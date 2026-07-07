namespace Backend.Domain.Entities;

public class UserBlock
{
    public int BlockerId { get; set; }
    public User Blocker { get; set; } = null!;

    public int BlockedId { get; set; }
    public User Blocked { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}