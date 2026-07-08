using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Backend.Application.DTOs.Requests.Posts;

public class CreatePostDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    public string? Content { get; set; }
}
