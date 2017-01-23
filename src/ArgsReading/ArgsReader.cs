using System;
using System.Collections.Generic;
using System.Linq;

namespace ArgsReading
{
	/// <summary>
	/// Helps process command-line arguments.
	/// </summary>
	/// <remarks>To use this class, construct an <c>ArgsReader</c> with the command-line arguments from <c>Main</c>,
	/// read the supported options one at a time with <see cref="ReadFlag" /> and <see cref="ReadOption"/>,
	/// read any normal arguments with <see cref="ReadArgument"/>, and finally call <see cref="VerifyComplete"/>,
	/// which throws an <see cref="ArgsReaderException"/> if any unsupported options or arguments haven't been read.</remarks>
	public sealed class ArgsReader
	{
		/// <summary>
		/// Creates a reader for the specified command-line arguments.
		/// </summary>
		/// <param name="args">The command-line arguments from <c>Main</c>.</param>
		public ArgsReader(IEnumerable<string> args)
		{
			if (args == null)
				throw new ArgumentNullException(nameof(args));

			m_args = args.ToList();
		}

		/// <summary>
		/// Reads the specified flag, returning true if it is found.
		/// </summary>
		/// <param name="name">The name of the specified flag.</param>
		/// <returns>True if the specified flag was found on the command line.</returns>
		/// <remarks><para>If the flag is found, the method returns <c>true</c> and the flag is
		/// removed. If <c>ReadFlag</c> is called with the same name, it will return <c>false</c>,
		/// unless the same flag appears twice on the command line.</para>
		/// <para>To support multiple names for the same flag, use a <c>|</c> to separate them,
		/// e.g. use <c>help|h|?</c> to support three different names for a help flag.</para>
		/// <para>Single-character names use a single hyphen, e.g. <c>-h</c>. Longer names use
		/// a double hyphen, e.g. <c>--help</c>.</para></remarks>
		/// <exception cref="ArgumentNullException"><c>name</c> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">One of the names is empty.</exception>
		public bool ReadFlag(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (name.Length == 0)
				throw new ArgumentException("Flag name must not be empty.", nameof(name));

			var names = name.Split('|');
			if (names.Length > 1)
				return names.Any(ReadFlag);

			int index = m_args.IndexOf(RenderOption(name));
			if (index == -1)
				return false;

			m_args.RemoveAt(index);
			return true;
		}

		/// <summary>
		/// Reads the value of the specified option, if any.
		/// </summary>
		/// <param name="name">The name of the specified option.</param>
		/// <returns>The specified option if it was found on the command line; <c>null</c> otherwise.</returns>
		/// <remarks><para>If the option is found, the method returns the command-line argument
		/// after the option and both arguments are removed. If <c>ReadOption</c> is called with the
		/// same name, it will return <c>null</c>, unless the same option appears twice on the command line.</para>
		/// <para>To support multiple names for the same option, use a <c>|</c> to separate them,
		/// e.g. use <c>n|name</c> to support two different names for a module option.</para>
		/// <para>Single-character names use a single hyphen, e.g. <c>-n example</c>. Longer names use
		/// a double hyphen, e.g. <c>--name example</c>.</para></remarks>
		/// <exception cref="ArgumentNullException"><c>name</c> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">One of the names is empty.</exception>
		/// <exception cref="ArgsReaderException">The argument that must follow the option is missing.</exception>
		public string ReadOption(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (name.Length == 0)
				throw new ArgumentException("Option name must not be empty.", nameof(name));

			var names = name.Split('|');
			if (names.Length > 1)
				return names.Select(ReadOption).FirstOrDefault(x => x != null);

			int index = m_args.IndexOf(RenderOption(name));
			if (index == -1)
				return null;

			string value = index + 1 < m_args.Count ? m_args[index + 1] : null;
			if (value == null || IsOption(value))
				throw new ArgsReaderException($"Missing value after '{RenderOption(name)}'.");

			m_args.RemoveAt(index);
			m_args.RemoveAt(index);
			return value;
		}

		/// <summary>
		/// Reads the next non-option argument.
		/// </summary>
		/// <returns>The next non-option argument, or null if none remain.</returns>
		/// <remarks><para>If the next argument is an option, this method throws an exception.
		/// If options can appear before normal arguments, be sure to read all options before reading
		/// any normal arguments.</para></remarks>
		/// <exception cref="ArgsReaderException">The next argument is an option.</exception>
		public string ReadArgument()
		{
			if (m_args.Count == 0)
				return null;

			string value = m_args[0];
			if (IsOption(value))
				throw new ArgsReaderException($"Unexpected option '{value}'.");

			m_args.RemoveAt(0);
			return value;
		}

		/// <summary>
		/// Confirms that all arguments were processed.
		/// </summary>
		/// <exception cref="ArgsReaderException">A command-line argument was not read.</exception>
		public void VerifyComplete()
		{
			if (m_args.Count != 0)
				throw new ArgsReaderException($"Unexpected {(IsOption(m_args[0]) ? "option" : "argument")} '{m_args[0]}'.");
		}

		private static bool IsOption(string value)
		{
			return value.Length >= 2 && value[0] == '-' && value != "--";
		}

		private static string RenderOption(string name)
		{
			return name.Length == 1 ? $"-{name}" : $"--{name}";
		}

		readonly List<string> m_args;
	}
}
