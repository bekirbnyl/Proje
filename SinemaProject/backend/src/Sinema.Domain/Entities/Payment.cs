using Sinema.Domain.Enums;

namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a payment transaction
/// </summary>
public class Payment
{
    /// <summary>
    /// Unique identifier for the payment
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Total amount paid
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment method used
    /// </summary>
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// Current status of the payment
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// External transaction reference (from payment provider)
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// When the payment was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the payment was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Collection of tickets associated with this payment
    /// </summary>
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    /// <summary>
    /// Marks the payment as successful
    /// </summary>
    /// <param name="externalReference">External transaction reference</param>
    public void MarkAsSucceeded(string? externalReference = null)
    {
        Status = PaymentStatus.Succeeded;
        ExternalReference = externalReference;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the payment as failed
    /// </summary>
    /// <param name="reason">Failure reason</param>
    public void MarkAsFailed(string? reason = null)
    {
        Status = PaymentStatus.Failed;
        ExternalReference = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the payment is completed (either succeeded or failed)
    /// </summary>
    public bool IsCompleted => Status != PaymentStatus.Pending;

    /// <summary>
    /// Checks if the payment was successful
    /// </summary>
    public bool IsSuccessful => Status == PaymentStatus.Succeeded;
}
