using FluentValidation;
using Sinema.Application.DTOs.Reports.Memberships;

namespace Sinema.Application.Validators.Reports;

/// <summary>
/// Validator for memberships report requests
/// </summary>
public class MembershipsReportRequestValidator : AbstractValidator<MembershipsReportRequest>
{
    /// <summary>
    /// Initializes a new instance of the MembershipsReportRequestValidator
    /// </summary>
    public MembershipsReportRequestValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty()
            .WithMessage("From date is required");

        RuleFor(x => x.To)
            .NotEmpty()
            .WithMessage("To date is required")
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("To date must be greater than or equal to From date");

        RuleFor(x => x)
            .Must(x => x.IsValidDateRange)
            .WithMessage("Date range cannot exceed 366 days");

        RuleFor(x => x.Grain)
            .NotEmpty()
            .WithMessage("Grain is required")
            .Must(x => MembershipsReportRequest.ValidGrains.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Grain must be one of: {string.Join(", ", MembershipsReportRequest.ValidGrains)}");

        RuleFor(x => x.Format)
            .NotEmpty()
            .WithMessage("Format is required")
            .Must(x => x.Equals("json", StringComparison.OrdinalIgnoreCase) || 
                      x.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Format must be either 'json' or 'xlsx'");
    }
}
