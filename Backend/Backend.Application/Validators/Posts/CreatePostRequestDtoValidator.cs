using FluentValidation;
using Backend.Application.DTOs.Requests.Posts;

namespace Backend.Application.Validators.Posts
{
    public class CreatePostRequestDtoValidator : AbstractValidator<CreatePostRequestDto>
    {
        public CreatePostRequestDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("İçerik boş olamaz.")
                .MaximumLength(1000).WithMessage("İçerik 1000 karakterden uzun olamaz.");

            // Either Content or ImageUrl must be present, but since Content is usually string?, let's just make it required or ensure one of them is there.
            // Wait, CreatePostRequestDto has: string Content, string? ImageUrl, bool IsVideo
            // So Content is not nullable in the record. Thus NotEmpty is fine.
        }
    }
}
