using Backend.Application.DTOs.Requests.Posts;
using FluentValidation;

namespace Backend.Application.Validators.Posts;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
    }
}
