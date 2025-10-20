using FluentValidation;

namespace Sinema.Application.Features.Halls.Commands.ActivateSeatLayoutVersion;

/// <summary>
/// Validator for ActivateSeatLayoutVersionCommand
/// </summary>
public class ActivateSeatLayoutVersionValidator : AbstractValidator<ActivateSeatLayoutVersionCommand>
{
    public ActivateSeatLayoutVersionValidator()
    {
        RuleFor(x => x.HallId)
            .NotEmpty()
            .WithMessage("Hall ID is required.");

        RuleFor(x => x.LayoutId)
            .NotEmpty()
            .WithMessage("Layout ID is required.");
    }
}
