using FluentValidation;

namespace Sinema.Application.Features.Reservations.Commands.CreateReservation;

/// <summary>
/// Validator for CreateReservationCommand
/// </summary>
public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.ScreeningId)
            .NotEmpty()
            .WithMessage("Screening ID is required.");

        RuleFor(x => x.SeatIds)
            .NotEmpty()
            .WithMessage("At least one seat ID is required.")
            .Must(seatIds => seatIds.Count <= 10)
            .WithMessage("Cannot reserve more than 10 seats at once.")
            .Must(seatIds => seatIds.Count == seatIds.Distinct().Count())
            .WithMessage("Duplicate seat IDs are not allowed.");

        RuleFor(x => x.ClientToken)
            .NotEmpty()
            .WithMessage("Client token is required.")
            .Length(1, 64)
            .WithMessage("Client token must be between 1 and 64 characters.");

        RuleForEach(x => x.SeatIds)
            .NotEmpty()
            .WithMessage("Seat ID cannot be empty.");
    }
}
