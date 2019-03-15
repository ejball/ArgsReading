using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using McMaster.Extensions.CommandLineUtils;

namespace BuildTools
{
	public static class BuildRunner
	{
		public static int Execute(string[] args, Action<BuildApp> initialize, string scriptPath = null, [CallerFilePath] string callerFilePath = null)
		{
			string scriptDirectory = Path.GetDirectoryName(scriptPath ?? callerFilePath);
			string buildDirectory = Path.GetFullPath(Path.Combine(scriptDirectory ?? ".", "..", ".."));
			Directory.SetCurrentDirectory(buildDirectory);

			var commandLineApp = new CommandLineApplication();

			var buildApp = new BuildApp(commandLineApp);
			initialize(buildApp);

			var helpFlag = buildApp.AddFlag("-h|-?|--help", "Show build help");
			var targetsArgument = commandLineApp.Argument("targets", "The targets to build", multipleValues: true);

			commandLineApp.OnExecute(() =>
			{
				var targets = targetsArgument.Values;
				if (helpFlag.Value || targets.Count == 0 && buildApp.Targets.All(x => x.Name != "default"))
				{
					commandLineApp.ShowHelp();
					ShowTargets(buildApp.Targets);
				}
				else
				{
					buildApp.RunTargets(targets);
				}
			});

			return commandLineApp.Execute(args);
		}

		private static void ShowTargets(IReadOnlyList<BuildTarget> targets)
		{
			Console.WriteLine("Targets:");
			foreach (var target in targets)
				Console.WriteLine("  {0}", target.Name);
		}
	}
}
