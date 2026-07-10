using Backend.Application.DTOs.Requests.Auth;
using FluentValidation;

namespace Backend.Application.Validators.Auth;

public class EnableTwoFactorRequestDtoValidator : AbstractValidator<EnableTwoFactorRequestDto>
{
    public EnableTwoFactorRequestDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be exactly 6 characters.");
    }
}
