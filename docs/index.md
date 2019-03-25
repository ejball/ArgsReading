# ArgsReading

**ArgsReading** is a .NET library for reading command-line arguments.

## Installation

Install **ArgsReading** using NuGet. [![NuGet](https://img.shields.io/nuget/v/ArgsReading.svg)](https://www.nuget.org/packages/ArgsReading)

**ArgsReading** is compatible with most .NET platforms via [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

## Usage

**ArgsReading** is considerably simpler (and less powerful) than most command-line argument libraries.

It does not support registering a list of options, data type conversion, binding options to the properties of a class, documenting options, or displaying usage. If you want those features, use a competing library such as [CommandLineParser](https://www.nuget.org/packages/CommandLineParser) or [McMaster.Extensions.CommandLineUtils](https://www.nuget.org/packages/McMaster.Extensions.CommandLineUtils/).

To use this library, construct an [ArgsReader](ArgsReading/ArgsReader/ArgsReader.md) with the command-line arguments, read the supported options one at a time with [ReadFlag](ArgsReading/ArgsReader/ReadFlag.md) and [ReadOption](ArgsReading/ArgsReader/ReadOption.md), read any normal arguments with [ReadArgument](ArgsReading/ArgsReader/ReadArgument.md), and finally call [VerifyComplete](ArgsReading/ArgsReader/VerifyComplete.md), which throws an [ArgsReaderException](ArgsReading/ArgsReaderException.md) if any unsupported options or arguments haven't been read.

For more information, consult the [reference documentation](ArgsReading.md).
