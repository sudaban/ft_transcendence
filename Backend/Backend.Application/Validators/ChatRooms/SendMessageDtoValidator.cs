using Backend.Application.DTOs.Requests.ChatRooms;
using FluentValidation;

namespace Backend.Application.Validators.ChatRooms;

public class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required.")
            .MaximumLength(4000).WithMessage("Message cannot exceed 4000 characters.");
    }
}
