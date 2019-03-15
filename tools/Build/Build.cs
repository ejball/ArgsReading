using System;
using BuildTools;
using static Bullseye.Targets;

internal static class Build
{
	public static int Main(string[] args) => BullseyeBuildRunner.Execute(args, app =>
	{
		DotNetBuild.CreateTargets(app,
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

		Target("default", DependsOn("test"));
	});
}
