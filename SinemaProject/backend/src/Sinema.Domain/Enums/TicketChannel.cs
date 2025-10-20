namespace Sinema.Domain.Enums;

/// <summary>
/// Represents the channel through which a ticket was sold
/// </summary>
public enum TicketChannel
{
    /// <summary>
    /// Sold through the web application (online)
    /// </summary>
    Online = 0,

    /// <summary>
    /// Sold at the cinema box office
    /// </summary>
    BoxOffice = 1,

    /// <summary>
    /// Sold through mobile application
    /// </summary>
    Mobile = 2
}
