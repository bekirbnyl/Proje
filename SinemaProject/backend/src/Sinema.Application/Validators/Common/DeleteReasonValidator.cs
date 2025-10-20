using FluentValidation;
using System.Text.RegularExpressions;

namespace Sinema.Application.Validators.Common;

/// <summary>
/// Validator for deletion reasons
/// </summary>
public class DeleteReasonValidator : AbstractValidator<string>
{
    private static readonly Regex HtmlTagRegex = new(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ScriptRegex = new(@"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public DeleteReasonValidator()
    {
        RuleFor(reason => reason)
            .NotEmpty()
            .WithMessage("Deletion reason is required")
            .MinimumLength(5)
            .WithMessage("Deletion reason must be at least 5 characters long")
            .MaximumLength(500)
            .WithMessage("Deletion reason cannot exceed 500 characters")
            .Must(BeValidReason)
            .WithMessage("Deletion reason contains invalid content or potential security risks");
    }

    /// <summary>
    /// Validates that the reason is safe and appropriate
    /// </summary>
    /// <param name="reason">Deletion reason to validate</param>
    /// <returns>True if valid</returns>
    private static bool BeValidReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return false;

        // Check for HTML tags (basic XSS prevention)
        if (HtmlTagRegex.IsMatch(reason))
            return false;

        // Check for script tags specifically
        if (ScriptRegex.IsMatch(reason))
            return false;

        // Check for common injection patterns
        var lowerReason = reason.ToLowerInvariant();
        if (lowerReason.Contains("javascript:") ||
            lowerReason.Contains("vbscript:") ||
            lowerReason.Contains("onload=") ||
            lowerReason.Contains("onerror=") ||
            lowerReason.Contains("eval(") ||
            lowerReason.Contains("expression("))
        {
            return false;
        }

        // Must contain at least some alphabetic characters (not just numbers/symbols)
        if (!reason.Any(char.IsLetter))
            return false;

        return true;
    }

    /// <summary>
    /// Sanitizes a deletion reason by removing HTML tags and trimming
    /// </summary>
    /// <param name="reason">Reason to sanitize</param>
    /// <returns>Sanitized reason</returns>
    public static string SanitizeReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return string.Empty;

        // Remove HTML tags
        var sanitized = HtmlTagRegex.Replace(reason, string.Empty);
        
        // Remove script content
        sanitized = ScriptRegex.Replace(sanitized, string.Empty);
        
        // Trim and normalize whitespace
        sanitized = Regex.Replace(sanitized.Trim(), @"\s+", " ");

        return sanitized;
    }

    /// <summary>
    /// Validates a deletion reason string
    /// </summary>
    /// <param name="reason">Reason to validate</param>
    /// <returns>Validation result</returns>
    public static (bool IsValid, string[] Errors) ValidateReason(string reason)
    {
        var validator = new DeleteReasonValidator();
        var result = validator.Validate(reason);
        
        return (result.IsValid, result.Errors.Select(e => e.ErrorMessage).ToArray());
    }
}
