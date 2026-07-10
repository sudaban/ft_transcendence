using Backend.Application.DTOs.Responses.Users;
using System;

namespace Backend.Application.DTOs.Responses.Posts;

public class CommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    public UserDto User { get; set; } = null!;
}
