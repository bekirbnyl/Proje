using FluentValidation;
using Sinema.Application.DTOs.Reports.Deletions;

namespace Sinema.Application.Validators.Reports;

/// <summary>
/// Validator for deletions report requests
/// </summary>
public class DeletionsReportRequestValidator : AbstractValidator<DeletionsReportRequest>
{
    /// <summary>
    /// Initializes a new instance of the DeletionsReportRequestValidator
    /// </summary>
    public DeletionsReportRequestValidator()
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

        RuleFor(x => x.By)
            .NotEmpty()
            .WithMessage("By parameter is required")
            .Must(x => DeletionsReportRequest.ValidByOptions.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"By parameter must be one of: {string.Join(", ", DeletionsReportRequest.ValidByOptions)}");

        RuleFor(x => x.Format)
            .NotEmpty()
            .WithMessage("Format is required")
            .Must(x => x.Equals("json", StringComparison.OrdinalIgnoreCase) || 
                      x.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Format must be either 'json' or 'xlsx'");
    }
}
