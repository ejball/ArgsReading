using McMaster.Extensions.CommandLineUtils;

namespace BuildTools
{
	public sealed class BuildOption
	{
		public string Value => m_option.HasValue() ? m_option.Value() : m_defaultValue;

		internal BuildOption(CommandOption option, string defaultValue)
		{
			m_option = option;
			m_defaultValue = defaultValue;
		}

		private readonly CommandOption m_option;
		private readonly string m_defaultValue;
	}
}
