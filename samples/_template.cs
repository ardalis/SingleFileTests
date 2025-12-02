// Single-file xUnit test suite for .NET 10+
// Run with: dotnet run test.cs
//
// Package references use the #:package syntax:
#:package xunit@2.9.2
#:package xunit.runner.utility@2.9.2
//
// Project references use the #:project syntax (reference projects to test):
// #:project ../path/to/MyProject
// #:project ../MyLibrary
//
// Note: Uncomment and update the #:project lines above to reference projects
// containing types you want to test. The tests themselves are defined in this file.

using Xunit;
using Xunit.Runners;

// ============================================================================
// Test Runner (Top-level statements must come before type declarations)
// ============================================================================

// Use Environment.ProcessPath for single-file apps (Assembly.Location returns empty string)
var assemblyPath = Environment.ProcessPath!;

Console.WriteLine("Discovering and running tests...\n");

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
await Task.Delay(100); // Allow runner to transition to idle state before disposal
runner.Dispose();

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
// Test Classes (Must be declared after top-level statements)
// ============================================================================

public class IntAdditionOperator
{
    [Fact]
    public void ReturnsCorrectSumGivenTwoIntegers()
    {
        Assert.Equal(4, 2 + 2);
    }
}
public class StringContains
{
    [Fact]
    public void ReturnsSubstringWhenPresent()
    {
        Assert.Contains("world", "Hello world!");
    }
}
public class IntModulo2GivenOddNumbers
{
    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void ReturnsTrue(int value)
    {
        Assert.True(value % 2 == 1);
    }
}

public class IntMultiplicationOperator
{
    [Fact]
    public void ReturnsZeroWhenMultipliedByZero()
    {
        Assert.Equal(0, 5 * 0);
        Assert.Equal(0, 0 * 5);
    }

    [Fact]
    public void ReturnsSameNumberWhenMultipliedByOne()
    {
        Assert.Equal(7, 7 * 1);
        Assert.Equal(7, 1 * 7);
    }

    [Fact]
    public void ReturnsCorrectProductGivenTwoPositiveIntegers()
    {
        Assert.Equal(6, 2 * 3);
        Assert.Equal(20, 4 * 5);
        Assert.Equal(100, 10 * 10);
    }
}

public class FailureDemonstration
{
    // Uncomment the [Fact] attribute below to demonstrate CI/CD failure behavior
    // When a test fails, the program exits with code 1, causing builds to fail
    // [Fact]
    public void FailingTest_DemonstrateCIFailure()
    {
        Assert.Equal(5, 2 + 2); // This will fail
    }
}

// ============================================================================
// Example: Testing an External Project
// ============================================================================
// To test types from an external project:
// 1. Add a #:project reference at the top of this file
// 2. Create test classes below that use types from that project
//
// Example:
// public class CalculatorAdd
// {
//     [Fact]
//     public void ReturnsSumGivenTwoIntegers()
//     {
//         var calc = new MyProject.Calculator();
//         Assert.Equal(5, calc.Add(2, 3));
//     }
// }
//
// The test runner will discover and run all tests defined in THIS file,
// even if they test types from external projects.
