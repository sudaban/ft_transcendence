using FluentValidation;
using Backend.Application.DTOs.Requests.Users;

namespace Backend.Application.Validators.Users
{
    public class UpdateProfileRequestDtoValidator : AbstractValidator<UpdateProfileRequestDto>
    {
        public UpdateProfileRequestDtoValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Bio)
                .MaximumLength(300).WithMessage("Biography cannot exceed 300 characters.");
        }
    }
}
