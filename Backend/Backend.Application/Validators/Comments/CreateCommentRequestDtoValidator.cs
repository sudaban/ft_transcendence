using FluentValidation;
using Backend.Application.DTOs.Requests.Comments;

namespace Backend.Application.Validators.Comments
{
    public class CreateCommentRequestDtoValidator : AbstractValidator<CreateCommentRequestDto>
    {
        public CreateCommentRequestDtoValidator()
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0).WithMessage("Please enter a valid post ID.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment content cannot be empty.")
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
        }
    }
}
