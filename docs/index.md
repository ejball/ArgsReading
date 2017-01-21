# ArgsReading

**ArgsReading** is a .NET library for reading command-line arguments.

## Usage

**ArgsReading** is considerably simpler (and less powerful) than most command-line argument libraries.

It does not support registering a list of options, data type conversion, binding options to the properties of a class, documenting options, or displaying usage. If you want those features, use a competing library such as [Command Line Parser Library](https://www.nuget.org/packages/commandlineparser).

To use this library, construct an [ArgsReader](ArgsReading/ArgsReader/ArgsReader) with the command-line arguments, read the supported options one at a time with [ReadFlag](ArgsReading/ArgsReader/ReadFlag) and [ReadOption](ArgsReading/ArgsReader/ReadOption), read any normal arguments with [ReadArgument](ArgsReading/ArgsReader/ReadArgument), and finally call [VerifyComplete](ArgsReading/ArgsReader/VerifyComplete), which throws an [ArgsReaderException](ArgsReading/ArgsReaderException) if any unsupported options or arguments haven't been read.

See also the [reference documentation](ArgsReading).

## Installation

Install **ArgsReading** from its [NuGet package](https://www.nuget.org/packages/ArgsReading).

**ArgsReading** is compatible with most .NET platforms via [.NET Standard 1.1](https://docs.microsoft.com/en-us/dotnet/articles/standard/library) and [Portable Class Library (Profile111)](https://docs.microsoft.com/en-us/nuget/schema/target-frameworks#portable-class-libraries).
