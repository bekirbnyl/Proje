using FluentValidation;
using Sinema.Application.DTOs.Auth;

namespace Sinema.Application.Validators.Auth;

/// <summary>
/// Validator for RefreshTokenRequest DTO
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required");
    }
}
