using System;

namespace Backend.Domain.Entities;

public class DeletedMessage
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;

    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}
