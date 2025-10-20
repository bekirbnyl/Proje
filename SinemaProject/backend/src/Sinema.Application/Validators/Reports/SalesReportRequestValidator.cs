using FluentValidation;
using Sinema.Application.DTOs.Reports.Sales;

namespace Sinema.Application.Validators.Reports;

/// <summary>
/// Validator for sales report requests
/// </summary>
public class SalesReportRequestValidator : AbstractValidator<SalesReportRequest>
{
    /// <summary>
    /// Initializes a new instance of the SalesReportRequestValidator
    /// </summary>
    public SalesReportRequestValidator()
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
            .Must(x => SalesReportRequest.ValidGrains.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Grain must be one of: {string.Join(", ", SalesReportRequest.ValidGrains)}");

        RuleFor(x => x.By)
            .NotEmpty()
            .WithMessage("By parameter is required")
            .Must(x => SalesReportRequest.ValidByOptions.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"By parameter must be one of: {string.Join(", ", SalesReportRequest.ValidByOptions)}");

        RuleFor(x => x.Channel)
            .NotEmpty()
            .WithMessage("Channel is required")
            .Must(x => SalesReportRequest.ValidChannels.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Channel must be one of: {string.Join(", ", SalesReportRequest.ValidChannels)}");

        RuleFor(x => x.Format)
            .NotEmpty()
            .WithMessage("Format is required")
            .Must(x => x.Equals("json", StringComparison.OrdinalIgnoreCase) || 
                      x.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Format must be either 'json' or 'xlsx'");
    }
}
