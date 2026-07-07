using FluentValidation;
using Backend.Application.DTOs.Requests.Posts;

namespace Backend.Application.Validators.Posts
{
    public class CreatePostRequestDtoValidator : AbstractValidator<CreatePostRequestDto>
    {
        public CreatePostRequestDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content cannot be empty.")
                .MaximumLength(1000).WithMessage("Content cannot exceed 1000 characters.");
        }
    }
}
