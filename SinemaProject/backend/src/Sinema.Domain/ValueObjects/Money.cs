namespace Sinema.Domain.ValueObjects;

/// <summary>
/// Represents a monetary value with currency
/// </summary>
public record Money
{
    /// <summary>
    /// The monetary amount
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// The currency code (currently fixed to TRY)
    /// </summary>
    public string Currency { get; init; } = "TRY";

    /// <summary>
    /// Creates a new Money instance
    /// </summary>
    /// <param name="amount">The monetary amount</param>
    /// <param name="currency">The currency code (default: TRY)</param>
    public Money(decimal amount, string currency = "TRY")
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a Money instance with zero amount
    /// </summary>
    public static Money Zero => new(0);

    /// <summary>
    /// Creates a Money instance from a decimal amount
    /// </summary>
    /// <param name="amount">The amount</param>
    /// <returns>Money instance</returns>
    public static Money From(decimal amount) => new(amount);

    /// <summary>
    /// Adds two Money instances
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot add different currencies: {left.Currency} and {right.Currency}");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    /// <summary>
    /// Subtracts two Money instances
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot subtract different currencies: {left.Currency} and {right.Currency}");
        }

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    /// <summary>
    /// Multiplies Money by a factor
    /// </summary>
    public static Money operator *(Money money, decimal factor) => new(money.Amount * factor, money.Currency);

    /// <summary>
    /// Checks if this money amount is greater than another
    /// </summary>
    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");
        }

        return left.Amount > right.Amount;
    }

    /// <summary>
    /// Checks if this money amount is less than another
    /// </summary>
    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");
        }

        return left.Amount < right.Amount;
    }

    /// <summary>
    /// String representation of the money value
    /// </summary>
    public override string ToString() => $"{Amount:C} {Currency}";
}
