using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Application.DTOs.Requests.Auth
{
    public record RegisterRequestDto
    (
        string Username,
        string Email,
        string Password,
        string? FullName
    );
}
