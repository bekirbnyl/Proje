using FluentValidation;

namespace Sinema.Application.Features.Settings.Commands.UpdateSettings;

/// <summary>
/// Validator for UpdateSettingsCommand
/// </summary>
public class UpdateSettingsValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsValidator()
    {
        RuleFor(x => x.Settings)
            .NotNull()
            .WithMessage("Settings collection cannot be null")
            .NotEmpty()
            .WithMessage("At least one setting must be provided");

        RuleForEach(x => x.Settings)
            .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
            .WithMessage("Setting key cannot be empty")
            .Must(kvp => kvp.Value != null)
            .WithMessage("Setting value cannot be null");

        RuleForEach(x => x.Settings)
            .Must(kvp => kvp.Key.Length <= 100)
            .WithMessage("Setting key cannot exceed 100 characters")
            .Must(kvp => kvp.Value.Length <= 1000)
            .WithMessage("Setting value cannot exceed 1000 characters");
    }
}
