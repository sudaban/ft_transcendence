using System.ComponentModel.DataAnnotations;

namespace Backend.Application.DTOs.Requests.ChatRooms;

public class SendMessageDto
{
    [Required(ErrorMessage = "Message content is required")]
    [StringLength(4000, ErrorMessage = "Message cannot exceed 4000 characters")]
    public string Content { get; set; } = null!;
}
