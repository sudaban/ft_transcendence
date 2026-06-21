using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Application.DTOs.Requests.Posts
{
    public record CreatePostRequestDto
    (
        string Content,
        string? ImageUrl,
        bool IsVideo
    );
}