namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a credit transaction for a member account
/// </summary>
public class MemberCredit
{
    /// <summary>
    /// Unique identifier for the credit transaction
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the member
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Credit amount (positive for credit, negative for debit)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Type of credit transaction
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Description of the transaction
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Reference to related payment or transaction
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// When the credit transaction was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the member
    /// </summary>
    public virtual Member Member { get; set; } = null!;

    /// <summary>
    /// Checks if this is a credit (positive amount)
    /// </summary>
    public bool IsCredit => Amount > 0;

    /// <summary>
    /// Checks if this is a debit (negative amount)
    /// </summary>
    public bool IsDebit => Amount < 0;

    /// <summary>
    /// Creates a credit transaction for bank transfer
    /// </summary>
    /// <param name="memberId">Member ID</param>
    /// <param name="amount">Credit amount</param>
    /// <param name="reference">Bank transfer reference</param>
    /// <returns>MemberCredit instance</returns>
    public static MemberCredit CreateBankTransferCredit(Guid memberId, decimal amount, string reference)
    {
        return new MemberCredit
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Amount = Math.Abs(amount), // Ensure positive
            Type = "BankTransfer",
            Description = "Credit top-up via bank transfer",
            Reference = reference,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a debit transaction for ticket purchase
    /// </summary>
    /// <param name="memberId">Member ID</param>
    /// <param name="amount">Debit amount</param>
    /// <param name="ticketId">Ticket reference</param>
    /// <returns>MemberCredit instance</returns>
    public static MemberCredit CreateTicketPurchaseDebit(Guid memberId, decimal amount, Guid ticketId)
    {
        return new MemberCredit
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Amount = -Math.Abs(amount), // Ensure negative
            Type = "TicketPurchase",
            Description = "Ticket purchase debit",
            Reference = ticketId.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a credit transaction for refund
    /// </summary>
    /// <param name="memberId">Member ID</param>
    /// <param name="amount">Refund amount</param>
    /// <param name="reason">Refund reason</param>
    /// <returns>MemberCredit instance</returns>
    public static MemberCredit CreateRefund(Guid memberId, decimal amount, string reason)
    {
        return new MemberCredit
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Amount = Math.Abs(amount), // Ensure positive
            Type = "Refund",
            Description = reason,
            CreatedAt = DateTime.UtcNow
        };
    }
}
