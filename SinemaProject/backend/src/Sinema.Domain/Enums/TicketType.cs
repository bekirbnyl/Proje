namespace Sinema.Domain.Enums;

/// <summary>
/// Represents the type of ticket and associated pricing rules
/// </summary>
public enum TicketType
{
    /// <summary>
    /// Full price ticket
    /// </summary>
    Full = 0,

    /// <summary>
    /// Student discount ticket (40% discount)
    /// </summary>
    Student = 1,

    /// <summary>
    /// VIP member ticket (special pricing rules apply)
    /// </summary>
    VIP = 2,

    /// <summary>
    /// VIP guest ticket (20% discount when accompanied by VIP member)
    /// </summary>
    VIPGuest = 3
}
