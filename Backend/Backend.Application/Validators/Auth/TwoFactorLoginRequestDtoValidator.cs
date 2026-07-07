using FluentValidation;
using Backend.Application.DTOs.Requests.Auth;

namespace Backend.Application.Validators.Auth
{
    public class TwoFactorLoginRequestDtoValidator : AbstractValidator<TwoFactorLoginRequestDto>
    {
        public TwoFactorLoginRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email address cannot be empty.")
                .EmailAddress().WithMessage("Please enter a valid email address.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Verification code cannot be empty.")
                .Length(6).WithMessage("Verification code must be 6 characters long.");

            RuleFor(x => x.TempToken)
                .NotEmpty().WithMessage("Temporary token cannot be empty.");
        }
    }
}
