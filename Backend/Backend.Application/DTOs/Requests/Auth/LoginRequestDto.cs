using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Application.DTOs.Requests.Auth
{
    public record LoginRequestDto
    (
        string Email,
        string Password
    );
    
}
