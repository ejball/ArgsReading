using System;
using FluentAssertions;
using Xunit;

namespace ArgsReading.Tests
{
	public class ArgsReaderTests
	{
		[Fact]
		public void NullCtorArgsThrowsException()
		{
			Invoking(() =>
			{
				var unused = new ArgsReader(null);
			}).ShouldThrow<ArgumentNullException>();
		}

		[Fact]
		public void ReadValidShortFlag()
		{
			var args = new ArgsReader(new[] { "-x" });
			args.ReadFlag("x").Should().BeTrue();
		}

		[Fact]
		public void ReadMissingShortFlag()
		{
			var args = new ArgsReader(new[] { "-y" });
			args.ReadFlag("x").Should().BeFalse();
		}

		[Fact]
		public void ReadShortFlagTwice()
		{
			var args = new ArgsReader(new[] { "-x" });
			args.ReadFlag("x").Should().BeTrue();
			args.ReadFlag("x").Should().BeFalse();
		}

		[Fact]
		public void ReadShortFlagTwiceSpecifiedTwice()
		{
			var args = new ArgsReader(new[] { "-x", "-x" });
			args.ReadFlag("x").Should().BeTrue();
			args.ReadFlag("x").Should().BeTrue();
		}

		[Fact]
		public void ReadValidLongFlag()
		{
			var args = new ArgsReader(new[] { "--xyzzy" });
			args.ReadFlag("xyzzy").Should().BeTrue();
		}

		[Fact]
		public void ReadShortOrLongFlagAsShort()
		{
			var args = new ArgsReader(new[] { "-x" });
			args.ReadFlag("x|xyzzy").Should().BeTrue();
		}

		[Fact]
		public void ReadShortOrLongFlagAsLong()
		{
			var args = new ArgsReader(new[] { "--xyzzy" });
			args.ReadFlag("x|xyzzy").Should().BeTrue();
		}

		[Fact]
		public void ReadValidOption()
		{
			var args = new ArgsReader(new[] { "-x", "whatever" });
			args.ReadOption("x").Should().Be("whatever");
		}

		[Fact]
		public void ReadMissingOption()
		{
			var args = new ArgsReader(new[] { "-y", "whatever" });
			args.ReadOption("x").Should().BeNull();
		}

		[Fact]
		public void ReadShortOrLongOptionAsShort()
		{
			var args = new ArgsReader(new[] { "-x", "whatever" });
			args.ReadOption("x|xyzzy").Should().Be("whatever");
		}

		[Fact]
		public void ReadShortOrLongOptionAsLong()
		{
			var args = new ArgsReader(new[] { "--xyzzy", "whatever" });
			args.ReadOption("x|xyzzy").Should().Be("whatever");
		}

		[Theory, InlineData(false), InlineData(true)]
		public void ReadShortOptionWrongCase(bool ignoreCase)
		{
			var args = new ArgsReader(new[] { "-X", "whatever" }) { ShortOptionIgnoreCase = ignoreCase };
			args.ReadOption("x").Should().Be(ignoreCase ? "whatever" : null);
		}

		[Theory, InlineData(false), InlineData(true)]
		public void ReadLongOptionWrongCase(bool ignoreCase)
		{
			var args = new ArgsReader(new[] { "--Xyzzy", "whatever" }) { LongOptionIgnoreCase = ignoreCase };
			args.ReadOption("xyzzY").Should().Be(ignoreCase ? "whatever" : null);
		}

		[Fact]
		public void ReadOptionMissingValue()
		{
			var args = new ArgsReader(new[] { "-x" });
			Invoking(() => args.ReadOption("x")).ShouldThrow<ArgsReaderException>();
		}

		[Fact]
		public void ReadOptionValueIsOption()
		{
			var args = new ArgsReader(new[] { "-x", "-y" });
			Invoking(() => args.ReadOption("x")).ShouldThrow<ArgsReaderException>();
		}

		[Fact]
		public void ReadValidArgument()
		{
			var args = new ArgsReader(new[] { "whatever" });
			args.ReadArgument().Should().Be("whatever");
		}

		[Fact]
		public void ReadMissingArgument()
		{
			var args = new ArgsReader(new string[0]);
			args.ReadArgument().Should().BeNull();
		}

		[Fact]
		public void ReadOptionAsArgument()
		{
			var args = new ArgsReader(new[] { "-x" });
			Invoking(() => args.ReadArgument()).ShouldThrow<ArgsReaderException>();
		}

		[Fact]
		public void VerifyComplete()
		{
			var args = new ArgsReader(new[] { "a", "-b", "-c", "d", "e" });
			args.ReadFlag("b").Should().BeTrue();
			args.ReadArgument().Should().Be("a");
			args.ReadOption("c").Should().Be("d");
			args.ReadArgument().Should().Be("e");
			args.VerifyComplete();
		}

		[Fact]
		public void VerifyIncomplete()
		{
			var args = new ArgsReader(new[] { "a" });
			Invoking(args.VerifyComplete).ShouldThrow<ArgsReaderException>();
		}

		private static Action Invoking(Action action) => action;
	}
}
