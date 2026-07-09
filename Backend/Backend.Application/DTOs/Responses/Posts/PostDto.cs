using Backend.Application.DTOs.Responses.Users;
using System;

namespace Backend.Application.DTOs.Responses.Posts;

public class PostDto
{
    public int Id { get; set; }
    public UserDto Author { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? Content { get; set; }
    public int ViewsCount { get; set; }
    public bool IsVideo { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
}