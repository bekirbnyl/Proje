namespace Sinema.Domain.Enums;

/// <summary>
/// Represents the status of a payment transaction
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending processing
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment has been successfully processed
    /// </summary>
    Succeeded = 1,

    /// <summary>
    /// Payment has failed or was declined
    /// </summary>
    Failed = 2
}
