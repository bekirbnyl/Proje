namespace Sinema.Domain.ValueObjects;

/// <summary>
/// Represents the position of a seat in a hall layout
/// </summary>
public record SeatPosition
{
    /// <summary>
    /// The row number (1-based)
    /// </summary>
    public int Row { get; init; }

    /// <summary>
    /// The column number (1-based)
    /// </summary>
    public int Col { get; init; }

    /// <summary>
    /// Creates a new SeatPosition
    /// </summary>
    /// <param name="row">Row number (1-based)</param>
    /// <param name="col">Column number (1-based)</param>
    public SeatPosition(int row, int col)
    {
        if (row <= 0)
        {
            throw new ArgumentException("Row must be greater than 0", nameof(row));
        }
        if (col <= 0)
        {
            throw new ArgumentException("Column must be greater than 0", nameof(col));
        }

        Row = row;
        Col = col;
    }

    /// <summary>
    /// Generates a human-readable seat label (e.g., "A1", "B5")
    /// </summary>
    public string ToLabel()
    {
        var rowLetter = ((char)('A' + Row - 1)).ToString();
        return $"{rowLetter}{Col}";
    }

    /// <summary>
    /// Creates a SeatPosition from a row letter and column number
    /// </summary>
    /// <param name="rowLetter">Row letter (A, B, C, etc.)</param>
    /// <param name="col">Column number</param>
    /// <returns>SeatPosition instance</returns>
    public static SeatPosition FromLabel(char rowLetter, int col)
    {
        var row = char.ToUpper(rowLetter) - 'A' + 1;
        return new SeatPosition(row, col);
    }

    /// <summary>
    /// String representation of the seat position
    /// </summary>
    public override string ToString() => $"Row {Row}, Col {Col} ({ToLabel()})";
}
