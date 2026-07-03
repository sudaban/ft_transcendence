using FluentValidation;
using Backend.Application.DTOs.Requests.Auth;

namespace Backend.Application.Validators.Auth
{
    public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address cannot be empty.")
                .EmailAddress().WithMessage("Please enter a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password cannot be empty.");
        }
    }
}
