using FluentValidation;
using Backend.Application.DTOs.Requests.Messages;

namespace Backend.Application.Validators.Messages
{
    public class SendMessageRequestDtoValidator : AbstractValidator<SendMessageRequestDto>
    {
        public SendMessageRequestDtoValidator()
        {
            RuleFor(x => x.ChatRoomId)
                .GreaterThan(0).WithMessage("Please enter a valid chat room ID.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Message cannot be empty.")
                .MaximumLength(2000).WithMessage("Message cannot exceed 2000 characters.");
        }
    }
}
