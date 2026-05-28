# ArgsReading

**ArgsReading** is a .NET library for reading command-line arguments.

[![NuGet](https://img.shields.io/nuget/v/ArgsReading.svg)](https://www.nuget.org/packages/ArgsReading)

## Usage

**ArgsReading** is considerably simpler (and less powerful) than most command-line argument libraries.

It does not support registering a list of options, data type conversion, binding options to the properties of a class, documenting options, or displaying usage. If you want those features, use a competing library such as [CommandLineParser](https://www.nuget.org/packages/CommandLineParser) or [McMaster.Extensions.CommandLineUtils](https://www.nuget.org/packages/McMaster.Extensions.CommandLineUtils/).

To use this library, construct an [ArgsReader](./src/ArgsReading/ArgsReader.cs) with the command-line arguments, read the supported options one at a time with [ReadFlag](./src/ArgsReading/ArgsReader.cs) and [ReadOption](./src/ArgsReading/ArgsReader.cs), read any normal arguments with [ReadArgument](./src/ArgsReading/ArgsReader.cs), and finally call [VerifyComplete](./src/ArgsReading/ArgsReader.cs), which throws an [ArgsReaderException](./src/ArgsReading/ArgsReaderException.cs) if any unsupported options or arguments haven't been read.

For more information, consult the [source code](./src/ArgsReading).
