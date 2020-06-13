using System;
using Faithlife.Build;

internal static class Build
{
	public static int Main(string[] args)
	{
		return BuildRunner.Execute(args, build =>
		{
			build.AddDotNetTargets(
				new DotNetBuildSettings
				{
					NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
					DocsSettings = new DotNetDocsSettings
					{
						GitLogin = new GitLoginInfo("ejball", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
						GitAuthor = new GitAuthorInfo("ejball", "ejball@gmail.com"),
						SourceCodeUrl = "https://github.com/ejball/ArgsReading/tree/master/src",
						GitBranchName = GetGitBranchName(),
					},
				});
		});

		string? GetGitBranchName()
		{
			const string prefix = "refs/heads/";
			return Environment.GetEnvironmentVariable("GITHUB_REF") is string githubRef && githubRef.StartsWith("refs/heads/", StringComparison.Ordinal) ? githubRef.Substring(prefix.Length) : null;
		}
	}
}
