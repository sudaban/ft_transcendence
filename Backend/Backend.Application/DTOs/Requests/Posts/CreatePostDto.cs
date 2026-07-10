using Microsoft.AspNetCore.Http;

namespace Backend.Application.DTOs.Requests.Posts;

public class CreatePostDto
{
    public IFormFile File { get; set; } = null!;

    public string? Content { get; set; }
}
