using FluentValidation;
using Backend.Application.DTOs.Requests.Users;

namespace Backend.Application.Validators.Users
{
    public class UpdateProfileRequestDtoValidator : AbstractValidator<UpdateProfileRequestDto>
    {
        public UpdateProfileRequestDtoValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("İsim 100 karakterden uzun olamaz.");

            RuleFor(x => x.Bio)
                .MaximumLength(300).WithMessage("Biyografi 300 karakterden uzun olamaz.");
        }
    }
}
