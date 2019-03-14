using McMaster.Extensions.CommandLineUtils;

namespace BuildTools
{
	public sealed class BuildApp
	{
		public BuildOption AddOption(string template, string description, string defaultValue = null) =>
			new BuildOption(m_app.Option(template, description, CommandOptionType.SingleValue), defaultValue);

		internal BuildApp(CommandLineApplication app)
		{
			m_app = app;
		}

		private readonly CommandLineApplication m_app;
	}
}
