using System.Text;
using System.Text.RegularExpressions;
using Sinema.Application.Abstractions.Tickets;

namespace Sinema.Infrastructure.Services.Tickets;

/// <summary>
/// Service for generating unique ticket codes in the format AB12-34CD
/// </summary>
public class TicketNumberGenerator : ITicketNumberGenerator
{
    private static readonly Random Random = new();
    private static readonly char[] Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] Numbers = "0123456789".ToCharArray();
    private static readonly Regex TicketCodePattern = new(@"^[A-Z]{2}\d{2}-\d{2}[A-Z]{2}$", RegexOptions.Compiled);

    public string GenerateTicketCode()
    {
        var sb = new StringBuilder(9); // AB12-34CD = 9 characters

        // First two letters
        sb.Append(Letters[Random.Next(Letters.Length)]);
        sb.Append(Letters[Random.Next(Letters.Length)]);

        // First two numbers
        sb.Append(Numbers[Random.Next(Numbers.Length)]);
        sb.Append(Numbers[Random.Next(Numbers.Length)]);

        // Separator
        sb.Append('-');

        // Second two numbers
        sb.Append(Numbers[Random.Next(Numbers.Length)]);
        sb.Append(Numbers[Random.Next(Numbers.Length)]);

        // Last two letters
        sb.Append(Letters[Random.Next(Letters.Length)]);
        sb.Append(Letters[Random.Next(Letters.Length)]);

        return sb.ToString();
    }

    public bool IsValidTicketCode(string ticketCode)
    {
        if (string.IsNullOrWhiteSpace(ticketCode))
        {
            return false;
        }

        return TicketCodePattern.IsMatch(ticketCode);
    }
}
