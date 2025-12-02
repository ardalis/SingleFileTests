// Single-file xUnit test suite for Money value object
// Run with: dotnet run moneytests.cs
//
#:package xunit@2.9.2
#:package xunit.runner.utility@2.9.2
#:project ../src/ValueObjects

using ValueObjects;
using Xunit;
using Xunit.Runners;

// ============================================================================
// Test Runner
// ============================================================================

var assemblyPath = Environment.ProcessPath!;

Console.WriteLine("Discovering and running Money tests...\n");

var runner = AssemblyRunner.WithoutAppDomain(assemblyPath);

int passed = 0, failed = 0, skipped = 0;
var startTime = DateTime.Now;
var consoleLock = new object();

runner.OnTestFailed = info =>
{
    lock (consoleLock)
    {
        failed++;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("  [FAIL] ");
        Console.ResetColor();
        Console.WriteLine(info.TestDisplayName);
        Console.WriteLine($"         {info.ExceptionMessage}");
        if (!string.IsNullOrEmpty(info.ExceptionStackTrace))
        {
            foreach (var line in info.ExceptionStackTrace.Split('\n'))
            {
                Console.WriteLine($"         {line}");
            }
        }
    }
};

runner.OnTestPassed = info =>
{
    lock (consoleLock)
    {
        passed++;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  [PASS] ");
        Console.ResetColor();
        Console.WriteLine(info.TestDisplayName);
    }
};

runner.OnTestSkipped = info =>
{
    lock (consoleLock)
    {
        skipped++;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("  [SKIP] ");
        Console.ResetColor();
        Console.WriteLine($"{info.TestDisplayName} - {info.SkipReason}");
    }
};

var tcs = new TaskCompletionSource<bool>();
runner.OnExecutionComplete = info => tcs.SetResult(true);

runner.Start();
await tcs.Task;
await Task.Delay(100);
runner.Dispose();

var duration = DateTime.Now - startTime;

Console.WriteLine($"\nTest run completed in {duration.TotalSeconds:F2}s");
Console.WriteLine($"Total tests: {passed + failed + skipped}");

if (passed > 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  Passed: {passed}");
    Console.ResetColor();
}

if (failed > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  Failed: {failed}");
    Console.ResetColor();
}

if (skipped > 0)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  Skipped: {skipped}");
    Console.ResetColor();
}

return failed > 0 ? 1 : 0;

// ============================================================================
// Money Value Object Tests
// ============================================================================

public class MoneyConstruction
{
    [Fact]
    public void CreatesMoneyWithAmountAndCurrency()
    {
        var money = new Money(100.50m, "USD");

        Assert.Equal(100.50m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void NormalizesCurrencyToUpperCase()
    {
        var money = new Money(50m, "eur");

        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void FactoryMethodsCreateCorrectCurrency()
    {
        var usd = Money.USD(100);
        var eur = Money.EUR(200);
        var gbp = Money.GBP(300);

        Assert.Equal("USD", usd.Currency);
        Assert.Equal("EUR", eur.Currency);
        Assert.Equal("GBP", gbp.Currency);
    }

    [Fact]
    public void ZeroCreatesZeroAmountWithCurrency()
    {
        var zero = Money.Zero("USD");

        Assert.Equal(0m, zero.Amount);
        Assert.Equal("USD", zero.Currency);
        Assert.True(zero.IsZero);
    }

    [Fact]
    public void ThrowsWhenCurrencyIsNullOrEmpty()
    {
        Assert.Throws<ArgumentNullException>(() => new Money(100, null!));
        Assert.Throws<ArgumentException>(() => new Money(100, ""));
        Assert.Throws<ArgumentException>(() => new Money(100, "   "));
    }
}

public class MoneyArithmetic
{
    [Fact]
    public void AddsTwoMoneyValuesWithSameCurrency()
    {
        var a = Money.USD(100);
        var b = Money.USD(50);

        var result = a + b;

        Assert.Equal(150m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void SubtractsTwoMoneyValuesWithSameCurrency()
    {
        var a = Money.USD(100);
        var b = Money.USD(30);

        var result = a - b;

        Assert.Equal(70m, result.Amount);
    }

    [Fact]
    public void MultipliesByDecimal()
    {
        var money = Money.USD(100);

        var result = money * 1.5m;

        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public void DividesByDecimal()
    {
        var money = Money.USD(100);

        var result = money / 4m;

        Assert.Equal(25m, result.Amount);
    }

    [Fact]
    public void ThrowsWhenAddingDifferentCurrencies()
    {
        var usd = Money.USD(100);
        var eur = Money.EUR(100);

        Assert.Throws<InvalidOperationException>(() => usd + eur);
    }

    [Fact]
    public void IncrementAddsOne()
    {
        var money = Money.USD(100);

        money++;

        Assert.Equal(101m, money.Amount);
    }

    [Fact]
    public void DecrementSubtractsOne()
    {
        var money = Money.USD(100);

        money--;

        Assert.Equal(99m, money.Amount);
    }

    [Fact]
    public void UnaryMinusNegatesAmount()
    {
        var money = Money.USD(100);

        var negated = -money;

        Assert.Equal(-100m, negated.Amount);
    }
}

public class MoneyEquality
{
    [Fact]
    public void EqualWhenSameAmountAndCurrency()
    {
        var a = Money.USD(100);
        var b = Money.USD(100);

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void NotEqualWhenDifferentAmount()
    {
        var a = Money.USD(100);
        var b = Money.USD(200);

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }

    [Fact]
    public void NotEqualWhenDifferentCurrency()
    {
        var a = Money.USD(100);
        var b = Money.EUR(100);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void HashCodeEqualForEqualMoney()
    {
        var a = Money.USD(100);
        var b = Money.USD(100);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}

public class MoneyComparison
{
    [Fact]
    public void ComparesAmountsWithSameCurrency()
    {
        var small = Money.USD(50);
        var large = Money.USD(100);

        Assert.True(small < large);
        Assert.True(large > small);
        Assert.True(small <= large);
        Assert.True(large >= small);
    }

    [Fact]
    public void ThrowsWhenComparingDifferentCurrencies()
    {
        var usd = Money.USD(100);
        var eur = Money.EUR(100);

        Assert.Throws<InvalidOperationException>(() => usd < eur);
    }
}

public class MoneyHelpers
{
    [Fact]
    public void RoundsToSpecifiedDecimals()
    {
        var money = new Money(100.556m, "USD");

        var rounded = money.Round(2);

        Assert.Equal(100.56m, rounded.Amount);
    }

    [Fact]
    public void AbsReturnsAbsoluteValue()
    {
        var negative = new Money(-100m, "USD");

        var absolute = negative.Abs();

        Assert.Equal(100m, absolute.Amount);
    }

    [Theory]
    [InlineData(100, false, true, false)]
    [InlineData(0, true, false, false)]
    [InlineData(-50, false, false, true)]
    public void IsZeroPositiveNegativeReturnCorrectValues(
        decimal amount, bool isZero, bool isPositive, bool isNegative)
    {
        var money = new Money(amount, "USD");

        Assert.Equal(isZero, money.IsZero);
        Assert.Equal(isPositive, money.IsPositive);
        Assert.Equal(isNegative, money.IsNegative);
    }

    [Fact]
    public void ToStringFormatsCorrectly()
    {
        var money = new Money(1234.56m, "USD");

        Assert.Equal("1,234.56 USD", money.ToString());
    }
}
