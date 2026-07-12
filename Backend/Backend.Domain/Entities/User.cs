using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? FullName { get; set; }

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

    public bool IsBanned { get; set; } = false;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsOnline { get; set; } = false;

    public DateTime? LastSeenAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Follow> FollowedBy { get; set; } = new HashSet<Follow>();
    public ICollection<Follow> Following { get; set; } = new HashSet<Follow>();
    public ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

    public ICollection<UserBlock> BlockedUsers { get; set; } = new HashSet<UserBlock>();
    public ICollection<UserBlock> BlockedBy { get; set; } = new HashSet<UserBlock>();

    public ICollection<ChatRoomMember> ChatRoomMemberships { get; set; } = new HashSet<ChatRoomMember>();

    public ICollection<PostLike> LikedPosts { get; set; } = new List<PostLike>();
    public ICollection<SavedPost> SavedPosts { get; set; } = new List<SavedPost>();
    public ICollection<DeletedMessage> DeletedMessages { get; set; } = new List<DeletedMessage>();
}