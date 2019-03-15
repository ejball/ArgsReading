using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using XmlDocMarkdown.Core;
using static BuildTools.BuildUtility;
using static BuildTools.DotNetRunner;

namespace BuildTools
{
	public static class DotNetBuild
	{
		public static void AddDotNetTargets(this BuildApp build, DotNetBuildSettings settings)
		{
			var configurationOption = build.AddOption("-c|--configuration <name>", "The configuration to build", "Release");
			var nugetApiKeyOption = build.AddOption("--nuget-api-key <name>", "NuGet API key for publishing");
			var versionSuffixOption = build.AddOption("--version-suffix <suffix>", "Generates a prerelease package");
			var triggerOption = build.AddOption("--trigger <name>", "The branch or tag that triggered the build");

			var solutionName = settings.SolutionName ?? throw new ArgumentException("Missing SolutionName.", nameof(settings));
			var nugetSource = settings.NuGetSource ?? "https://api.nuget.org/v3/index.json";

			var docsProjects = settings.XmlDocOutputSettings?.Projects;
			var docsRepoUrl = settings.XmlDocOutputSettings?.RepoUrl;
			var docsSourceUrl = settings.XmlDocOutputSettings?.SourceUrl;

			var buildBotUserName = settings.BuildBotSettings?.UserName;
			var buildBotPassword = settings.BuildBotSettings?.Password;
			var buildBotDisplayName = settings.BuildBotSettings?.DisplayName;
			var buildBotEmail = settings.BuildBotSettings?.Email;

			SetDotNetToolsDirectory(settings.DotNetToolsDirectory ?? "tools/bin");

			const string docsBranchName = "gh-pages";

			build.Target("clean")
				.Describe("Deletes all build output")
				.Does(() =>
				{
					foreach (var directory in FindDirectories("{src,tests}/**/{bin,obj}", "release"))
						Directory.Delete(directory, recursive: true);
				});

			build.Target("restore")
				.Describe("Restores NuGet packages")
				.Does(() => RunDotNet("restore", solutionName));

			build.Target("build")
				.DependsOn("restore")
				.Describe("Builds the solution")
				.Does(() => RunDotNet("build", solutionName, "-c", configurationOption.Value, "--no-restore", "--verbosity", "normal"));

			build.Target("rebuild")
				.DependsOn("clean", "build")
				.Describe("Cleans and builds the solution");

			build.Target("test")
				.DependsOn("build")
				.Describe("Runs the unit tests")
				.Does(() => RunDotNet("test", solutionName, "-c", configurationOption.Value, "--no-build"));

			build.Target("package")
				.DependsOn("rebuild", "test")
				.Describe("Builds the NuGet packages")
				.Does(() =>
				{
					string versionSuffix = versionSuffixOption.Value;
					string trigger = triggerOption.Value;
					if (versionSuffix == null && trigger != null)
					{
						var group = Regex.Match(trigger, @"^v[^\.]+\.[^\.]+\.[^\.]+-(.+)").Groups[1];
						if (group.Success)
							versionSuffix = group.ToString();
					}

					RunDotNet("pack", solutionName,
						"-c", configurationOption.Value,
						"--no-build",
						"--output", Path.GetFullPath("release"),
						versionSuffix != null ? "--version-suffix" : null, versionSuffix);
				});

			build.Target("package-test")
				.DependsOn("package")
				.Describe("Tests the NuGet packages")
				.Does(() =>
				{
					foreach (var packagePath in FindFiles("release/*.nupkg"))
						RunDotNetTool("sourcelink", "test", packagePath);
				});

			build.Target("docs")
				.DependsOn("build")
				.Describe("Generates reference documentation")
				.Does(() =>
				{
					if (docsProjects != null && docsProjects.Count != 0 && docsRepoUrl != null && docsSourceUrl != null)
					{
						if (!Directory.Exists(docsBranchName))
							Repository.Clone(docsRepoUrl, docsBranchName, new CloneOptions { BranchName = docsBranchName });

						foreach (string docsProject in docsProjects)
						{
							string dllPath = FindFiles($"src/{docsProject}/bin/**/{docsProject}.dll").First();
							XmlDocMarkdownGenerator.Generate(dllPath, $"{docsBranchName}/",
								new XmlDocMarkdownSettings { SourceCodePath = $"{docsSourceUrl}/{docsProject}", NewLine = "\n", ShouldClean = true });
						}
					}
				});

			build.Target("publish")
				.DependsOn("package-test", "docs")
				.Describe("Publishes the NuGet packages")
				.Does(() =>
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

						if (Directory.Exists(docsBranchName) &&
							Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master" &&
							!version.Contains("-"))
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
		}
	}
}
