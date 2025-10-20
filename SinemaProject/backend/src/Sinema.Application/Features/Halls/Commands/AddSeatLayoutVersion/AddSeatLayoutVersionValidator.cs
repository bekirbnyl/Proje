using FluentValidation;
using Sinema.Application.DTOs.Seating;

namespace Sinema.Application.Features.Halls.Commands.AddSeatLayoutVersion;

/// <summary>
/// Validator for AddSeatLayoutVersionCommand
/// </summary>
public class AddSeatLayoutVersionValidator : AbstractValidator<AddSeatLayoutVersionCommand>
{
    public AddSeatLayoutVersionValidator()
    {
        RuleFor(x => x.HallId)
            .NotEmpty()
            .WithMessage("Hall ID is required.");

        RuleFor(x => x.Seats)
            .NotEmpty()
            .WithMessage("At least one seat must be provided.");

        RuleFor(x => x.Seats)
            .Must(HaveUniquePositions)
            .WithMessage("Seat positions (row, column) must be unique within the layout.");

        RuleForEach(x => x.Seats)
            .ChildRules(seat =>
            {
                seat.RuleFor(s => s.Row)
                    .GreaterThan(0)
                    .WithMessage("Row must be greater than 0.");

                seat.RuleFor(s => s.Col)
                    .GreaterThan(0)
                    .WithMessage("Column must be greater than 0.");

                seat.RuleFor(s => s.Label)
                    .NotEmpty()
                    .WithMessage("Seat label is required.")
                    .MaximumLength(10)
                    .WithMessage("Seat label cannot exceed 10 characters.");
            });
    }

    private static bool HaveUniquePositions(IEnumerable<CreateSeatDto> seats)
    {
        var positions = seats.Select(s => new { s.Row, s.Col }).ToList();
        return positions.Count == positions.Distinct().Count();
    }
}
