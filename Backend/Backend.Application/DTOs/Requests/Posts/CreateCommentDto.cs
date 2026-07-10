using System.ComponentModel.DataAnnotations;

namespace Backend.Application.DTOs.Requests.Posts;

public class CreateCommentDto
{
    [Required(ErrorMessage = "Comment content is required")]
    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string Content { get; set; } = null!;
}
