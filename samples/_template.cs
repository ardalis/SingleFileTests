// Single-file xUnit v3 test suite for .NET 10+
// Run with: dotnet run test.cs
//
// Package references use the #:package syntax:
#:package xunit.v3@3.2.1
#:package xunit.v3.runner.inproc.console@3.2.1
//
// Project references use the #:project syntax (reference projects to test):
// #:project ../path/to/MyProject
// #:project ../MyLibrary
//
// Note: Uncomment and update the #:project lines above to reference projects
// containing types you want to test. The tests themselves are defined in this file.

using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole;
using Xunit.Sdk;
using System.Reflection;

// ============================================================================
// Test Runner (Top-level statements must come before type declarations)
// ============================================================================

// Use Environment.ProcessPath for single-file apps (Assembly.Location returns empty string)
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
