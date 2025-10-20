using FluentValidation;

namespace Sinema.Application.Features.Reservations.Holds.Commands.CreateSeatHolds;

/// <summary>
/// Validator for CreateSeatHoldsCommand
/// </summary>
public class CreateSeatHoldsValidator : AbstractValidator<CreateSeatHoldsCommand>
{
    public CreateSeatHoldsValidator()
    {
        RuleFor(x => x.ScreeningId)
            .NotEmpty()
            .WithMessage("Screening ID is required.");

        RuleFor(x => x.SeatIds)
            .NotEmpty()
            .WithMessage("At least one seat ID is required.")
            .Must(seatIds => seatIds.Count <= 10)
            .WithMessage("Cannot hold more than 10 seats at once.")
            .Must(seatIds => seatIds.Count == seatIds.Distinct().Count())
            .WithMessage("Duplicate seat IDs are not allowed.");

        RuleFor(x => x.ClientToken)
            .NotEmpty()
            .WithMessage("Client token is required.")
            .Length(1, 64)
            .WithMessage("Client token must be between 1 and 64 characters.");

        RuleFor(x => x.TtlSeconds)
            .GreaterThan(0)
            .WithMessage("TTL must be greater than 0 seconds.")
            .LessThanOrEqualTo(3600) // Max 1 hour
            .WithMessage("TTL cannot exceed 3600 seconds (1 hour).")
            .When(x => x.TtlSeconds.HasValue);

        RuleForEach(x => x.SeatIds)
            .NotEmpty()
            .WithMessage("Seat ID cannot be empty.");
    }
}
