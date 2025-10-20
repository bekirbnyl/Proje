using Sinema.Domain.Enums;

namespace Sinema.Application.Abstractions.Payments;

/// <summary>
/// Gateway for processing payments
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Authorizes and captures a payment
    /// </summary>
    /// <param name="amount">Amount to charge</param>
    /// <param name="method">Payment method</param>
    /// <param name="memberId">ID of the member (optional)</param>
    /// <param name="metadata">Additional metadata for the transaction</param>
    /// <returns>Payment result</returns>
    Task<PaymentResult> AuthorizeAndCaptureAsync(
        decimal amount, 
        PaymentMethod method, 
        Guid? memberId = null, 
        Dictionary<string, string>? metadata = null);
}

/// <summary>
/// Result of a payment transaction
/// </summary>
public class PaymentResult
{
    /// <summary>
    /// Whether the payment was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// External transaction ID from the payment provider
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Error message if payment failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata from the payment provider
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Creates a successful payment result
    /// </summary>
    public static PaymentResult Success(string transactionId, Dictionary<string, string>? metadata = null)
    {
        return new PaymentResult
        {
            IsSuccess = true,
            TransactionId = transactionId,
            Metadata = metadata ?? new()
        };
    }

    /// <summary>
    /// Creates a failed payment result
    /// </summary>
    public static PaymentResult Failure(string errorMessage)
    {
        return new PaymentResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
