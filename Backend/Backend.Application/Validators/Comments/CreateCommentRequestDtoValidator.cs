using FluentValidation;
using Backend.Application.DTOs.Requests.Comments;

namespace Backend.Application.Validators.Comments
{
    public class CreateCommentRequestDtoValidator : AbstractValidator<CreateCommentRequestDto>
    {
        public CreateCommentRequestDtoValidator()
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0).WithMessage("Geçerli bir gönderi ID'si giriniz.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Yorum içeriği boş olamaz.")
                .MaximumLength(500).WithMessage("Yorum 500 karakterden uzun olamaz.");
        }
    }
}
