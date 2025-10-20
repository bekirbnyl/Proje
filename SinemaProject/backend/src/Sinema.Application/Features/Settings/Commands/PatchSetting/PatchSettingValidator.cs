using FluentValidation;

namespace Sinema.Application.Features.Settings.Commands.PatchSetting;

/// <summary>
/// Validator for PatchSettingCommand
/// </summary>
public class PatchSettingValidator : AbstractValidator<PatchSettingCommand>
{
    public PatchSettingValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("Setting key is required")
            .MaximumLength(100)
            .WithMessage("Setting key cannot exceed 100 characters");

        RuleFor(x => x.Value)
            .NotNull()
            .WithMessage("Setting value cannot be null")
            .MaximumLength(1000)
            .WithMessage("Setting value cannot exceed 1000 characters");

        // Row version is optional but must be valid base64 if provided
        When(x => x.RowVersion != null, () =>
        {
            RuleFor(x => x.RowVersion)
                .Must(rv => rv!.Length > 0)
                .WithMessage("Row version cannot be empty if provided");
        });
    }
}
