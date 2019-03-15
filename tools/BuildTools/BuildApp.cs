using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace BuildTools
{
	public sealed class BuildApp
	{
		public IReadOnlyList<BuildTarget> Targets => m_targets;

		public BuildFlag AddFlag(string template, string description) =>
			new BuildFlag(m_app.Option(template, description, CommandOptionType.NoValue));

		public BuildOption AddOption(string template, string description, string defaultValue = null) =>
			new BuildOption(m_app.Option(template, description, CommandOptionType.SingleValue), defaultValue);

		public BuildTarget AddTarget(string template, Action action = null)
		{
			string[] nameEtc = template.Split(new[] { ':' }, 2);
			string name = nameEtc[0].Trim();
			var dependencies = nameEtc.Length == 1 ? new string[0] : nameEtc[1].Split(default(char[]), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

			Bullseye.Targets.Target(name, dependencies, action);

			var target = new BuildTarget(name, dependencies);
			m_targets.Add(target);
			return target;
		}

		public void RunTargets(IEnumerable<string> targets)
		{
			Bullseye.Targets.RunTargetsAndExit(targets);
		}

		internal BuildApp(CommandLineApplication app)
		{
			m_app = app;
			m_targets = new List<BuildTarget>();
		}

		private readonly CommandLineApplication m_app;
		private readonly List<BuildTarget> m_targets;
	}
}
