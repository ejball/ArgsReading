using System;
using BuildTools;

internal static class Build
{
	public static int Main(string[] args) => BullseyeBuildRunner.Execute(args, build =>
	{
		build.AddDotNetTargets(
			new DotNetBuildSettings
			{
				SolutionName = "ArgsReading.sln",
				XmlDocOutputSettings = new XmlDocOutputSettings
				{
					Projects = new[] { "ArgsReading" },
					RepoUrl = "https://github.com/ejball/ArgsReading.git",
					SourceUrl = "https://github.com/ejball/ArgsReading/tree/master/src",
				},
				BuildBotSettings = new BuildBotSettings
				{
					UserName = "ejball",
					Password = Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD"),
					DisplayName = "ejball",
					Email = "ejball@gmail.com",
				},
			});

		build.AddTarget("default : test");
	});
}
