using System;
using System.Linq;
using Bullseye;
using McMaster.Extensions.CommandLineUtils;

namespace BuildTools
{
	public sealed class BuildApp
	{
		public BuildFlag AddFlag(string template, string description) =>
			new BuildFlag(m_app.Option(template, description, CommandOptionType.NoValue));

		public BuildOption AddOption(string template, string description, string defaultValue = null) =>
			new BuildOption(m_app.Option(template, description, CommandOptionType.SingleValue), defaultValue);

		public void AddTarget(string template, Action action = null)
		{
			string[] nameEtc = template.Split(new[] { ':' }, 2);
			string name = nameEtc[0].Trim();
			var dependsOn = nameEtc.ElementAtOrDefault(1)?.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
			Targets.Target(name, dependsOn, action);
		}

		internal BuildApp(CommandLineApplication app)
		{
			m_app = app;
		}

		private readonly CommandLineApplication m_app;
	}
}
