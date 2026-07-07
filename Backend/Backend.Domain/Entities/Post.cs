namespace Backend.Domain.Entities;

public class Post
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string? Content { get; set; }

    public int ViewsCount { get; set; } = 0;

    public bool IsVideo { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PostLike> Likes { get; set; } = new HashSet<PostLike>();

    public ICollection<SavedPost> SavedByUsers { get; set; } = new HashSet<SavedPost>();

    public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
}