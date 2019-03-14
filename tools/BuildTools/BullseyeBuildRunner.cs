using System;
using System.Runtime.CompilerServices;
using static Bullseye.Targets;

namespace BuildTools
{
	public static class BullseyeBuildRunner
	{
		public static int Execute(string[] args, Action<BuildApp> initialize, string scriptPath = null, [CallerFilePath] string callerFilePath = null) =>
			BuildRunner.Execute(args, initialize, RunTargetsAndExit, scriptPath ?? callerFilePath);
	}
}
