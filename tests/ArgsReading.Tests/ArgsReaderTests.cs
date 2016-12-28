using System;
using NUnit.Framework;
using Shouldly;

namespace ArgsReading.Tests
{
	[TestFixture]
	public class ArgsReaderTests
	{
		[Test]
		public void NullCtorArgsThrowsException()
		{
			Assert.Throws<ArgumentNullException>(() => new ArgsReader(null));
		}

		[Test]
		public void ReadValidShortFlag()
		{
			var args = new ArgsReader(new[] { "-x" });
			args.ReadFlag("x").ShouldBe(true);
		}

		[Test]
		public void ReadMissingShortFlag()
		{
			var args = new ArgsReader(new[] { "-y" });
			args.ReadFlag("x").ShouldBe(false);
		}

		[Test]
		public void ReadShortFlagTwice()
		{
			var args = new ArgsReader(new[] { "-x" });
			args.ReadFlag("x").ShouldBe(true);
			args.ReadFlag("x").ShouldBe(false);
		}

		[Test]
		public void ReadShortFlagTwiceSpecifiedTwice()
		{
			var args = new ArgsReader(new[] { "-x", "-x" });
			args.ReadFlag("x").ShouldBe(true);
			args.ReadFlag("x").ShouldBe(true);
		}

		[Test]
		public void ReadValidLongFlag()
		{
			var args = new ArgsReader(new[] { "--xyzzy" });
			args.ReadFlag("xyzzy").ShouldBe(true);
		}

		[Test]
		public void ReadShortOrLongFlagAsShort()
		{
			var args = new ArgsReader(new[] { "-x" });
			args.ReadFlag("x|xyzzy").ShouldBe(true);
		}

		[Test]
		public void ReadShortOrLongFlagAsLong()
		{
			var args = new ArgsReader(new[] { "--xyzzy" });
			args.ReadFlag("x|xyzzy").ShouldBe(true);
		}

		[Test]
		public void ReadValidOption()
		{
			var args = new ArgsReader(new[] { "-x", "whatever" });
			args.ReadOption("x").ShouldBe("whatever");
		}

		[Test]
		public void ReadMissingOption()
		{
			var args = new ArgsReader(new[] { "-y", "whatever" });
			args.ReadOption("x").ShouldBe(null);
		}

		[Test]
		public void ReadShortOrLongOptionAsShort()
		{
			var args = new ArgsReader(new[] { "-x", "whatever" });
			args.ReadOption("x|xyzzy").ShouldBe("whatever");
		}

		[Test]
		public void ReadShortOrLongOptionAsLong()
		{
			var args = new ArgsReader(new[] { "--xyzzy", "whatever" });
			args.ReadOption("x|xyzzy").ShouldBe("whatever");
		}

		[Test]
		public void ReadOptionMissingValue()
		{
			var args = new ArgsReader(new[] { "-x" });
			Assert.Throws<ArgsReaderException>(() => args.ReadOption("x"));
		}

		[Test]
		public void ReadOptionValueIsOption()
		{
			var args = new ArgsReader(new[] { "-x", "-y" });
			Assert.Throws<ArgsReaderException>(() => args.ReadOption("x"));
		}

		[Test]
		public void ReadValidArgument()
		{
			var args = new ArgsReader(new[] { "whatever" });
			args.ReadArgument().ShouldBe("whatever");
		}

		[Test]
		public void ReadMissingArgument()
		{
			var args = new ArgsReader(new string[0]);
			args.ReadArgument().ShouldBe(null);
		}

		[Test]
		public void ReadOptionAsArgument()
		{
			var args = new ArgsReader(new[] { "-x" });
			Assert.Throws<ArgsReaderException>(() => args.ReadArgument());
		}

		[Test]
		public void VerifyComplete()
		{
			var args = new ArgsReader(new[] { "a", "-b", "-c", "d", "e" });
			args.ReadFlag("b").ShouldBe(true);
			args.ReadArgument().ShouldBe("a");
			args.ReadOption("c").ShouldBe("d");
			args.ReadArgument().ShouldBe("e");
			args.VerifyComplete();
		}

		[Test]
		public void VerifyIncomplete()
		{
			var args = new ArgsReader(new[] { "a" });
			Assert.Throws<ArgsReaderException>(() => args.VerifyComplete());
		}
	}
}
