namespace ValueObjects;

/// <summary>
/// A value object representing a monetary amount with currency.
/// Demonstrates C# 14 features including operators defined directly on types.
/// </summary>
public readonly struct Money : IEquatable<Money>, IComparable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    // Zero value for a given currency
    public static Money Zero(string currency) => new(0, currency);

    // Common currency factory methods
    public static Money USD(decimal amount) => new(amount, "USD");
    public static Money EUR(decimal amount) => new(amount, "EUR");
    public static Money GBP(decimal amount) => new(amount, "GBP");

    // Arithmetic operators
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator checked +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return checked(new Money(left.Amount + right.Amount, left.Currency));
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator checked -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return checked(new Money(left.Amount - right.Amount, left.Currency));
    }

    public static Money operator ++(Money value) => new(value.Amount + 1, value.Currency);

    public static Money operator --(Money value) => new(value.Amount - 1, value.Currency);

    public static Money operator *(Money left, decimal multiplier) =>
        new(left.Amount * multiplier, left.Currency);

    public static Money operator *(decimal multiplier, Money right) =>
        new(right.Amount * multiplier, right.Currency);

    public static Money operator /(Money left, decimal divisor) =>
        new(left.Amount / divisor, left.Currency);

    public static Money operator %(Money left, decimal divisor) =>
        new(left.Amount % divisor, left.Currency);

    public static Money operator checked *(Money left, decimal multiplier) =>
        checked(new Money(left.Amount * multiplier, left.Currency));

    public static Money operator checked /(Money left, decimal divisor) =>
        checked(new Money(left.Amount / divisor, left.Currency));

    // Unary operators
    public static Money operator +(Money value) => value;
    public static Money operator -(Money value) => new(-value.Amount, value.Currency);

    // Comparison operators
    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);

    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount <= right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount >= right.Amount;
    }

    // IEquatable<Money>
    public bool Equals(Money other) =>
        Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) =>
        obj is Money other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Amount, Currency);

    // IComparable<Money>
    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    // Display
    public override string ToString() => $"{Amount:N2} {Currency}";

    public string ToString(string? format) =>
        string.IsNullOrEmpty(format)
            ? ToString()
            : $"{Amount.ToString(format)} {Currency}";

    // Helper methods
    public Money Round(int decimals = 2) =>
        new(Math.Round(Amount, decimals, MidpointRounding.AwayFromZero), Currency);

    public Money Abs() => new(Math.Abs(Amount), Currency);

    public bool IsZero => Amount == 0;
    public bool IsPositive => Amount > 0;
    public bool IsNegative => Amount < 0;

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException(
                $"Cannot perform operation on Money with different currencies: {left.Currency} and {right.Currency}");
        }
    }
}
