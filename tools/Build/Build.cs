using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GlobExpressions;
using McMaster.Extensions.CommandLineUtils;
using SimpleExec;
using XmlDocMarkdown.Core;
using static Bullseye.Targets;

internal sealed class Build
{
	[Option("-c|--configuration", ValueName = "name", Description = "The configuration to build (default Release)")]
	public string Configuration { get; } = "Release";

	[Option("--nuget-api-key", ValueName = "key", Description = "NuGet API key for publishing")]
	public string NuGetApiKey { get; } = null;

	[Option("--version-suffix", ValueName = "suffix", Description = "Generates a prerelease package")]
	public string VersionSuffix { get; } = null;

	[Option("--trigger", ValueName = "name", Description = "The branch or tag that triggered the build")]
	public string Trigger { get; } = null;

	[Argument(0, Description = "The targets to build")]
	public string[] Targets { get; } = { };

	public const string SolutionName = "ArgsReading.sln";
	public const string NuGetSource = "https://api.nuget.org/v3/index.json";
	public readonly IReadOnlyList<string> DocsProjects = new[] { "ArgsReading" };
	public const string DocsSourceUri = "https://github.com/ejball/ArgsReading/tree/master/src";

	public void CreateTargets()
	{
		Target("clean",
			() =>
			{
				foreach (var directory in FindDirectories("{src,tests}/**/{bin,obj}", "release"))
					Directory.Delete(directory, recursive: true);
			});

		Target("restore",
			() => RunDotNet("restore", SolutionName));

		Target("build",
			DependsOn("restore"),
			() => RunDotNet("build", SolutionName, "-c", Configuration, "--no-restore", "--verbosity", "normal"));

		Target("rebuild",
			DependsOn("clean", "build"));

		Target("test",
			DependsOn("build"),
			() => RunDotNet("test", SolutionName, "-c", Configuration, "--no-build"));

		Target("package",
			DependsOn("rebuild", "test"),
			() =>
			{
				string versionSuffix = VersionSuffix;
				if (versionSuffix == null && Trigger != null)
				{
					var group = Regex.Match(Trigger, @"^v[^\.]+\.[^\.]+\.[^\.]+-(.+)").Groups[1];
					if (group.Success)
						versionSuffix = group.ToString();
				}

				RunDotNet("pack", SolutionName,
					"-c", Configuration,
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

		Target("publish",
			DependsOn("package-test"),
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

				if (NuGetApiKey != null && (Trigger == null || Regex.IsMatch(Trigger, "^v[0-9]")))
				{
					if (Trigger != null && Trigger != $"v{version}")
						throw new InvalidOperationException($"Trigger '{Trigger}' doesn't match package version '{version}'.");
					foreach (var nupkgPath in nupkgPaths)
						RunDotNet("nuget", "push", nupkgPath, "--source", NuGetSource, "--api-key", NuGetApiKey);
				}
				else
				{
					Console.WriteLine($"To publish this package, push this git tag: v{version}");
				}
			});

		Target("docs",
			DependsOn("build"),
			() =>
			{
				foreach (string docsProject in DocsProjects)
				{
					XmlDocMarkdownGenerator.Generate($"src/{docsProject}/bin/{Configuration}/netstandard2.0/{docsProject}.dll", "release/docs",
						new XmlDocMarkdownSettings { SourceCodePath = $"{DocsSourceUri}/{docsProject}", NewLine = "\n", ShouldClean = true });
				}
			});

		Target("default",
			DependsOn("test"));
	}

	private static async Task Main(string[] args) => await CommandLineApplication.ExecuteAsync<Build>(args);

	private async Task OnExecuteAsync(CommandLineApplication app)
	{
		string directory = GetSolutionDirectory();
		Directory.SetCurrentDirectory(directory);
		if (!File.Exists(SolutionName))
			throw new InvalidOperationException($"Missing solution file {SolutionName} at {directory}.");

		CreateTargets();

		await RunTargetsAndExitAsync(Targets);
	}

	private static string GetScriptDirectory([CallerFilePath] string filePath = null) => Path.GetDirectoryName(filePath);

	private static string GetSolutionDirectory() => Path.GetFullPath(Path.Combine(GetScriptDirectory(), "..", ".."));

	private static void RunApp(string path, params string[] args) => Command.Run(path, ArgumentEscaper.EscapeAndConcatenate(args.Where(x => x != null)));

	private static void RunDotNet(params string[] args) => RunApp(DotNetExe.FullPath, args);

	private static void RunDotNetTool(string name, params string[] args)
	{
		string toolsPath = Path.Combine("tools", "bin");
		if (!File.Exists(Path.Combine(toolsPath, $"{name}.exe")))
			RunDotNet("tool", "install", name, "--tool-path", toolsPath);
		RunApp(Path.Combine(toolsPath, name), args);
	}

	private static IReadOnlyList<string> FindDirectories(params string[] globs) => globs.SelectMany(glob => Glob.Directories(".", glob)).ToList();

	private static IReadOnlyList<string> FindFiles(params string[] globs) => globs.SelectMany(glob => Glob.Files(".", glob)).ToList();
}
