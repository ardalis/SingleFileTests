// Single-file xUnit v3 test suite for Money value object
// Run with: dotnet run moneytests.cs
//
#:package xunit.v3@3.2.1
#:package xunit.v3.runner.inproc.console@3.2.1
#:project ../src/ValueObjects

using ValueObjects;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole;
using Xunit.Sdk;
using System.Reflection;

// ============================================================================
// Test Runner (Top-level statements must come before type declarations)
// ============================================================================

// Use Environment.ProcessPath for single-file apps (Assembly.Location returns empty string).
// For test scenarios under test hosts, ProcessPath points to the test host, not the test DLL.
// Use Assembly.Location as fallback only if ProcessPath is not a DLL.
var processPath = Environment.ProcessPath!;

// For dotnet run, the executable is a .dll or .exe with corresponding .dll
string assemblyPath;
if (processPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
{
    assemblyPath = processPath;
}
else if (processPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
{
    // Convert .exe to .dll path (dotnet produces both)
    assemblyPath = Path.ChangeExtension(processPath, ".dll");
}
else
{
    // Single-file compilation - try to find a dll with same name
    assemblyPath = processPath + ".dll";
}

Console.WriteLine("Discovering and running tests...\n");

int passed = 0, failed = 0, skipped = 0;
var startTime = DateTime.Now;
var consoleLock = new object();

// Track test display names using test unique IDs
var testNameMap = new Dictionary<string, string>();

var sink = new TestMessageSink();

// Track test names as they start
sink.Execution.TestStartingEvent += args =>
{
    testNameMap[args.Message.TestUniqueID] = args.Message.TestDisplayName;
};

sink.Execution.TestFailedEvent += args =>
{
    lock (consoleLock)
    {
        failed++;
        var testName = testNameMap.TryGetValue(args.Message.TestUniqueID, out var name) ? name : "Unknown test";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("  [FAIL] ");
        Console.ResetColor();
        Console.WriteLine(testName);
        if (args.Message.Messages != null && args.Message.Messages.Length > 0 && args.Message.Messages[0] != null)
        {
            Console.WriteLine($"         {args.Message.Messages[0]}");
        }
        if (args.Message.StackTraces != null && args.Message.StackTraces.Length > 0 && args.Message.StackTraces[0] != null && !string.IsNullOrEmpty(args.Message.StackTraces[0]))
        {
            foreach (var line in args.Message.StackTraces[0]!.Split('\n'))
            {
                Console.WriteLine($"         {line}");
            }
        }
    }
};

sink.Execution.TestPassedEvent += args =>
{
    lock (consoleLock)
    {
        passed++;
        var testName = testNameMap.TryGetValue(args.Message.TestUniqueID, out var name) ? name : "Unknown test";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  [PASS] ");
        Console.ResetColor();
        Console.WriteLine(testName);
    }
};

sink.Execution.TestSkippedEvent += args =>
{
    lock (consoleLock)
    {
        skipped++;
        var testName = testNameMap.TryGetValue(args.Message.TestUniqueID, out var name) ? name : "Unknown test";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("  [SKIP] ");
        Console.ResetColor();
        Console.WriteLine($"{testName} - {args.Message.Reason}");
    }
};

// Run tests using ConsoleRunnerInProcess
var cts = new CancellationTokenSource();
#pragma warning disable IL2026 // Assembly.LoadFrom is required for test discovery
var assembly = Assembly.LoadFrom(assemblyPath);
#pragma warning restore IL2026

// Create XunitProjectAssembly for the test assembly
var project = new XunitProject();
var targetFramework = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName 
    ?? $".NETCoreApp,Version=v{Environment.Version.Major}.{Environment.Version.Minor}";
var metadata = new AssemblyMetadata(3, targetFramework); // xUnit v3
var projectAssembly = new XunitProjectAssembly(project, assemblyPath, metadata)
{
    Assembly = assembly,
    ConfigFileName = null,
};

await ConsoleRunnerInProcess.Run(sink, sink.Diagnostics, projectAssembly, cts);

var duration = DateTime.Now - startTime;

// Display results
Console.WriteLine($"\nTest run completed in {duration.TotalSeconds:F2}s");
Console.WriteLine($"Total tests: {passed + failed + skipped}");

if (passed > 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write($"  Passed: {passed}");
    Console.ResetColor();
    Console.WriteLine();
}

if (failed > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write($"  Failed: {failed}");
    Console.ResetColor();
    Console.WriteLine();
}

if (skipped > 0)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"  Skipped: {skipped}");
    Console.ResetColor();
    Console.WriteLine();
}

// Exit with appropriate code for CI/CD integration
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
