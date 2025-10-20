using FluentValidation;

namespace Sinema.Application.Features.Screenings.Commands.CreateScreening;

/// <summary>
/// Validator for CreateScreeningCommand
/// </summary>
public class CreateScreeningValidator : AbstractValidator<CreateScreeningCommand>
{
    public CreateScreeningValidator()
    {
        RuleFor(x => x.MovieId)
            .NotEmpty()
            .WithMessage("Movie ID is required.");

        RuleFor(x => x.HallId)
            .NotEmpty()
            .WithMessage("Hall ID is required.");

        RuleFor(x => x.StartAt)
            .NotEmpty()
            .WithMessage("Start time is required.")
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("Start time cannot be in the past.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than 0 minutes.")
            .LessThanOrEqualTo(600)
            .WithMessage("Duration cannot exceed 600 minutes.")
            .When(x => x.DurationMinutes.HasValue);
    }
}
