namespace Sinema.Domain.Enums;

/// <summary>
/// Represents the method used for payment
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Payment made by cash
    /// </summary>
    Cash = 0,

    /// <summary>
    /// Payment made by credit card
    /// </summary>
    CreditCard = 1,

    /// <summary>
    /// Payment made by bank transfer (for credit top-up)
    /// </summary>
    BankTransfer = 2,

    /// <summary>
    /// Payment made using member credit balance
    /// </summary>
    MemberCredit = 3
}
