using FluentValidation;

namespace Sinema.Application.Features.Movies.Commands.CreateMovie;

/// <summary>
/// Validator for CreateMovieCommand
/// </summary>
public class CreateMovieValidator : AbstractValidator<CreateMovieCommand>
{
    public CreateMovieValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Movie title is required.")
            .MinimumLength(2)
            .WithMessage("Movie title must be at least 2 characters long.")
            .MaximumLength(200)
            .WithMessage("Movie title cannot exceed 200 characters.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Movie duration must be greater than 0 minutes.")
            .LessThanOrEqualTo(600)
            .WithMessage("Movie duration cannot exceed 600 minutes.");
    }
}
