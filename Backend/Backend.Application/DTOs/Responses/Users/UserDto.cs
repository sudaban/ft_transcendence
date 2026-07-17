namespace Backend.Application.DTOs.Responses.Users;

public record UserDto
(
    string Id,          
    string Username,
    string Handle,      
    string Avatar,      
    string? FullName,
    string? Bio,
    int FollowersCount,
    int FollowingCount,
    int PostsCount,
    bool IsTwoFactorEnabled,
    bool IsOnline,
    DateTime? LastSeenAt,
    bool IsAiAssistant = false,
    bool IsBanned = false,
    bool IsDeleted = false
);