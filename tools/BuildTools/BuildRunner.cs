using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using McMaster.Extensions.CommandLineUtils;

namespace BuildTools
{
	public static class BuildRunner
	{
		public static int Execute(string[] args, Action<BuildApp> initialize, Action<IReadOnlyList<string>> run, string scriptPath = null, [CallerFilePath] string callerFilePath = null)
		{
			string scriptDirectory = Path.GetDirectoryName(scriptPath ?? callerFilePath);
			string buildDirectory = Path.GetFullPath(Path.Combine(scriptDirectory ?? ".", "..", ".."));
			Directory.SetCurrentDirectory(buildDirectory);

			var commandLineApp = new CommandLineApplication();

			var buildApp = new BuildApp(commandLineApp);
			initialize(buildApp);

			var helpOption = commandLineApp.Option("-h|-?|--help", "Show build help", CommandOptionType.NoValue);
			var arguments = commandLineApp.Argument("targets", "The targets to build", multipleValues: true);

			commandLineApp.OnExecute(() =>
			{
				if (helpOption.HasValue())
					commandLineApp.ShowHelp();
				else
					run(arguments.Values);
			});

			return commandLineApp.Execute(args);
		}
	}
}
