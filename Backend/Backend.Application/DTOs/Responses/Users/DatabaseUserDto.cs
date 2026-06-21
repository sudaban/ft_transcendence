namespace Backend.Application.DTOs.Responses.Users;

public record DatabaseUserDto(
    string Id,
    string Username,
    string? FullName,
    string Email,
    int FollowersCount,
    string? FollowRequestTime,
    IReadOnlyCollection<string> Following,
    IReadOnlyCollection<string> BlockedUsers,
    IReadOnlyCollection<string> LikedPosts,
    IReadOnlyCollection<string> SavedPosts,
    IReadOnlyCollection<string> CommentedPosts,
    IReadOnlyCollection<string> ActiveDMs,
    IReadOnlyCollection<string> Posts,
    bool TermsAccepted,
    bool IsDeleted,
    string RegistrationDate
);