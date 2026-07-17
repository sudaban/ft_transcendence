using Backend.Application.DTOs.Requests.Posts;
using FluentValidation;

namespace Backend.Application.Validators.Posts;

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    { 
        RuleFor(x => x.Content)
            .MaximumLength(2000).WithMessage("Post content cannot exceed 2000 characters.");
    }
}
