using Backend.Application.DTOs.Requests.Auth;
using FluentValidation;

namespace Backend.Application.Validators.Auth;

public class OAuthLoginRequestDtoValidator : AbstractValidator<OAuthLoginRequestDto>
{
    public OAuthLoginRequestDtoValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("OAuth provider is required.");
            
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("OAuth code is required.");
            
        RuleFor(x => x.RedirectUri)
            .NotEmpty().WithMessage("Redirect URI is required.");
    }
}
