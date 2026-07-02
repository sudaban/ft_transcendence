using FluentValidation;
using Backend.Application.DTOs.Requests.Auth;

namespace Backend.Application.Validators.Auth
{
    public class TwoFactorLoginRequestDtoValidator : AbstractValidator<TwoFactorLoginRequestDto>
    {
        public TwoFactorLoginRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email adresi boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Doğrulama kodu boş olamaz.")
                .Length(6).WithMessage("Doğrulama kodu 6 haneli olmalıdır.");

            RuleFor(x => x.TempToken)
                .NotEmpty().WithMessage("Geçici token boş olamaz.");
        }
    }
}
