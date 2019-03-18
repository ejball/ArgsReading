using System;
using Faithlife.Build;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		build.AddDotNetTargets(
			new DotNetBuildSettings
			{
				DocsSettings = new DocsSettings
				{
					Projects = new[] { "ArgsReading" },
					RepoUrl = "https://github.com/ejball/ArgsReading.git",
					SourceUrl = "https://github.com/ejball/ArgsReading/tree/master/src",
				},
				GitLogin = new GitLoginInfo("ejball", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
				GitAuthor = new GitAuthorInfo("ejball", "ejball@gmail.com"),
			});
	});
}
