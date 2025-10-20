using FluentValidation;

namespace Sinema.Application.Features.Movies.Commands.UpdateMovie;

/// <summary>
/// Validator for UpdateMovieCommand
/// </summary>
public class UpdateMovieValidator : AbstractValidator<UpdateMovieCommand>
{
    public UpdateMovieValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Movie ID is required.");

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
