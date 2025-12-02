# SingleFileTests

[![MIT License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Some examples of single file .NET tests using xUnit.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

## Running the Samples

All of the samples are run with just `dotnet run <filename.cs>`. If you prefer, on unix systems you should be able to `chmod +x <filename.cs>` and then run them using just `./filename.cs`.

Note that by default the tests run in parallel, so they will not be output in any particular order.

## Examples

The examples are in the [/samples directory](samples/).

### _template.cs

This file shows a bunch of different features in one file so that you can just grab it and tweak it to suit your needs. It includes comments that should help you figure out what you need to change.

Run the sample:

```powershell
dotnet run _template.cs
```

Expected output:

```powershell
> dotnet run _template.cs
Discovering and running tests...

  [PASS] IntModulo2GivenOddNumbers.ReturnsTrue(value: 5)
  [PASS] IntAdditionOperator.ReturnsCorrectSumGivenTwoIntegers
  [PASS] IntMultiplicationOperator.ReturnsSameNumberWhenMultipliedByOne
  [PASS] IntModulo2GivenOddNumbers.ReturnsTrue(value: 7)
  [PASS] IntModulo2GivenOddNumbers.ReturnsTrue(value: 3)
  [PASS] IntMultiplicationOperator.ReturnsCorrectProductGivenTwoPositiveIntegers
  [PASS] IntMultiplicationOperator.ReturnsZeroWhenMultipliedByZero
  [PASS] StringContains.ReturnsSubstringWhenPresent

Test run completed in 0.17s
Total tests: 8
  Passed: 8
```

