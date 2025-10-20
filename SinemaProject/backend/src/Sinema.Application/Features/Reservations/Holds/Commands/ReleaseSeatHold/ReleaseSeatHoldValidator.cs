using FluentValidation;

namespace Sinema.Application.Features.Reservations.Holds.Commands.ReleaseSeatHold;

/// <summary>
/// Validator for ReleaseSeatHoldCommand
/// </summary>
public class ReleaseSeatHoldValidator : AbstractValidator<ReleaseSeatHoldCommand>
{
    public ReleaseSeatHoldValidator()
    {
        RuleFor(x => x.HoldId)
            .NotEmpty()
            .WithMessage("Hold ID is required.");

        RuleFor(x => x.ClientToken)
            .NotEmpty()
            .WithMessage("Client token is required.")
            .Length(1, 64)
            .WithMessage("Client token must be between 1 and 64 characters.");
    }
}
