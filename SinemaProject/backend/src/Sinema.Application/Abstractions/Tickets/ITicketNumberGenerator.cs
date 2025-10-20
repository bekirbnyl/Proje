namespace Sinema.Application.Abstractions.Tickets;

/// <summary>
/// Service for generating unique ticket codes
/// </summary>
public interface ITicketNumberGenerator
{
    /// <summary>
    /// Generates a unique ticket code (e.g., "AB12-34CD")
    /// </summary>
    /// <returns>A unique, human-readable ticket code</returns>
    string GenerateTicketCode();

    /// <summary>
    /// Validates if a ticket code has the correct format
    /// </summary>
    /// <param name="ticketCode">The ticket code to validate</param>
    /// <returns>True if the format is valid</returns>
    bool IsValidTicketCode(string ticketCode);
}
