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

### moneytests.cs

This sample demonstrates how to test an external project using the `#:project` directive. It tests the `Money` value object from the included `ValueObjects` library located in `/src/ValueObjects`.

Run the sample:

```powershell
cd samples
dotnet run moneytests.cs
```

Expected output:

```powershell
Discovering and running Money tests...

  [PASS] MoneyArithmetic.AddsTwoMoneyValuesWithSameCurrency
  [PASS] MoneyConstruction.CreatesMoneyWithAmountAndCurrency
  [PASS] MoneyEquality.EqualWhenSameAmountAndCurrency
  ... (25 tests total)

Test run completed in 0.18s
Total tests: 25
  Passed: 25
```

## Using in GitHub Actions

You can run single file test apps in GitHub actions as long as you're on .NET 10 or later. Just use the `dotnet run <filename.cs>` syntax as shown here:

```yaml
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
        dotnet-quality: 'preview'

    - name: Run template tests
      run: dotnet run samples/_template.cs
```

See also [examples in .github\workflows](.github/workflows/).
