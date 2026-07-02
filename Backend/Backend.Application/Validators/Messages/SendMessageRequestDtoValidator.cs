using FluentValidation;
using Backend.Application.DTOs.Requests.Messages;

namespace Backend.Application.Validators.Messages
{
    public class SendMessageRequestDtoValidator : AbstractValidator<SendMessageRequestDto>
    {
        public SendMessageRequestDtoValidator()
        {
            RuleFor(x => x.ChatRoomId)
                .GreaterThan(0).WithMessage("Geçerli bir sohbet odası ID'si giriniz.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Mesaj boş olamaz.")
                .MaximumLength(2000).WithMessage("Mesaj en fazla 2000 karakter olabilir.");
        }
    }
}
