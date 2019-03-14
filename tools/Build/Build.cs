using System;
using System.IO;
using System.Text.RegularExpressions;
using BuildTools;
using LibGit2Sharp;
using XmlDocMarkdown.Core;
using static BuildTools.BuildUtility;
using static BuildTools.DotNetRunner;
using static Bullseye.Targets;

internal static class Build
{
	public static int Main(string[] args) => BullseyeBuildRunner.Execute(args, app =>
	{
		var configurationOption = app.AddOption("-c|--configuration <name>", "The configuration to build", "Release");
		var nugetApiKeyOption = app.AddOption("--nuget-api-key <name>", "NuGet API key for publishing");
		var versionSuffixOption = app.AddOption("--version-suffix <suffix>", "Generates a prerelease package");
		var triggerOption = app.AddOption("--trigger <name>", "The branch or tag that triggered the build");

		var solutionName = "ArgsReading.sln";
		var nugetSource = "https://api.nuget.org/v3/index.json";

		var docsProjects = new[] { "ArgsReading" };
		var docsRepoUri = "https://github.com/ejball/ArgsReading.git";
		var docsSourceUri = "https://github.com/ejball/ArgsReading/tree/master/src";

		var buildBotUserName = "ejball";
		var buildBotPassword = Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD");
		var buildBotDisplayName = "ejball";
		var buildBotEmail = "ejball@gmail.com";

		SetDotNetToolsDirectory("tools/bin");

		Target("clean",
			() =>
			{
				foreach (var directory in FindDirectories("{src,tests}/**/{bin,obj}", "release"))
					Directory.Delete(directory, recursive: true);
			});

		Target("restore",
			() => RunDotNet("restore", solutionName));

		Target("build",
			DependsOn("restore"),
			() => RunDotNet("build", solutionName, "-c", configurationOption.Value, "--no-restore", "--verbosity", "normal"));

		Target("rebuild",
			DependsOn("clean", "build"));

		Target("test",
			DependsOn("build"),
			() => RunDotNet("test", solutionName, "-c", configurationOption.Value, "--no-build"));

		Target("package",
			DependsOn("rebuild", "test"),
			() =>
			{
				string versionSuffix = versionSuffixOption.Value;
				string trigger = triggerOption.Value;
				if (versionSuffix == null && trigger != null)
				{
					var group = Regex.Match(trigger, @"^v[^\.]+\.[^\.]+\.[^\.]+-(.+)").Groups[1];
					if (@group.Success)
						versionSuffix = @group.ToString();
				}

				RunDotNet("pack", solutionName,
					"-c", configurationOption.Value,
					"--no-build",
					"--output", Path.GetFullPath("release"),
					versionSuffix != null ? "--version-suffix" : null, versionSuffix);
			});

		Target("package-test",
			DependsOn("package"),
			() =>
			{
				foreach (var packagePath in FindFiles("release/*.nupkg"))
					RunDotNetTool("sourcelink", "test", packagePath);
			});

		const string docsBranchName = "gh-pages";

		Target("docs",
			DependsOn("build"),
			() =>
			{
				if (!Directory.Exists(docsBranchName))
					Repository.Clone(docsRepoUri, docsBranchName, new CloneOptions { BranchName = docsBranchName });

				foreach (string docsProject in docsProjects)
				{
					XmlDocMarkdownGenerator.Generate($"src/{docsProject}/bin/{configurationOption.Value}/netstandard2.0/{docsProject}.dll", $"{docsBranchName}/",
						new XmlDocMarkdownSettings { SourceCodePath = $"{docsSourceUri}/{docsProject}", NewLine = "\n", ShouldClean = true });
				}
			});

		Target("publish",
			DependsOn("package-test", "docs"),
			() =>
			{
				var nupkgPaths = FindFiles("release/*.nupkg");

				string version = null;
				foreach (var nupkgPath in nupkgPaths)
				{
					string nupkgVersion = Regex.Match(nupkgPath, @"\.([^\.]+\.[^\.]+\.[^\.]+)\.nupkg$").Groups[1].ToString();
					if (version == null)
						version = nupkgVersion;
					else if (version != nupkgVersion)
						throw new InvalidOperationException($"Mismatched package versions '{version}' and '{nupkgVersion}'.");
				}

				var nugetApiKey = nugetApiKeyOption.Value;
				var trigger = triggerOption.Value;
				if (version != null && nugetApiKey != null && (trigger == null || Regex.IsMatch(trigger, "^v[0-9]")))
				{
					if (trigger != null && trigger != $"v{version}")
						throw new InvalidOperationException($"Trigger '{trigger}' doesn't match package version '{version}'.");
					foreach (var nupkgPath in nupkgPaths)
						RunDotNet("nuget", "push", nupkgPath, "--source", nugetSource, "--api-key", nugetApiKey);

					if (Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master" && !version.Contains("-"))
					{
						using (var repository = new Repository(docsBranchName))
						{
							if (repository.RetrieveStatus().IsDirty)
							{
								Console.WriteLine("Publishing documentation changes.");
								Commands.Stage(repository, "*");
								var author = new Signature(buildBotDisplayName, buildBotEmail, DateTimeOffset.Now);
								repository.Commit(message: "Automatic documentation update.", author, author, new CommitOptions());
								var credentials = new UsernamePasswordCredentials { Username = buildBotUserName, Password = buildBotPassword };
								repository.Network.Push(repository.Branches, new PushOptions { CredentialsProvider = (_, __, ___) => credentials });
							}
							else
							{
								Console.WriteLine("No documentation changes detected.");
							}
						}
					}
					else
					{
						Console.WriteLine("Documentation not published for this build.");
					}
				}
				else
				{
					Console.WriteLine($"To publish this package, push this git tag: v{version}");
				}
			});

		Target("default",
			DependsOn("test"));
	});
}
