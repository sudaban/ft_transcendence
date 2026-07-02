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
                .MaximumLength(100).WithMessage("Oda adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.ParticipantIds)
                .NotEmpty().WithMessage("Sohbet odasına en az bir katılımcı eklemelisiniz.")
                .Must(x => x != null && x.Any()).WithMessage("Katılımcılar listesi boş olamaz.");
        }
    }
}
