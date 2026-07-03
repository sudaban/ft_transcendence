using FluentValidation;
using Backend.Application.DTOs.Requests.Messages;
using System.Linq;

namespace Backend.Application.Validators.Messages
{
    public class CreateChatRoomRequestDtoValidator : AbstractValidator<CreateChatRoomRequestDto>
    {
        public CreateChatRoomRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Room name cannot exceed 100 characters.");

            RuleFor(x => x.ParticipantIds)
                .NotEmpty().WithMessage("You must add at least one participant to the chat room.")
                .Must(x => x != null && x.Any()).WithMessage("Participants list cannot be empty.");
        }
    }
}
