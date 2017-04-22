#addin "nuget:?package=Cake.Git&version=0.14.0"
#addin "nuget:?package=Octokit&version=0.24.0"
#tool "nuget:?package=XmlDocMarkdown&version=0.4.1"
#tool "nuget:?package=xunit.runner.console&version=2.2.0"

using LibGit2Sharp;
using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetApiKey = Argument("nugetApiKey", "");
var githubApiKey = Argument("githubApiKey", "");

var solutionFileName = "ArgsReading.sln";
var nugetSource = "https://api.nuget.org/v3/index.json";
var githubOwner = "ejball";
var githubRepo = "ArgsReading";
var docsAssembly = $@"src\ArgsReading\bin\{configuration}\netstandard1.1\ArgsReading.dll";
var docsSourceUri = "https://github.com/ejball/ArgsReading/tree/master/src/ArgsReading";

var rootPath = MakeAbsolute(Directory(".")).FullPath;
var gitRepository = LibGit2Sharp.Repository.IsValid(rootPath) ? new LibGit2Sharp.Repository(rootPath) : null;

var githubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("build.cake"));
if (!string.IsNullOrEmpty(githubApiKey))
	githubClient.Credentials = new Octokit.Credentials(githubApiKey);

Task("Clean")
	.Does(() =>
	{
		CleanDirectories("src/**/bin");
		CleanDirectories("src/**/obj");
		CleanDirectories("tests/**/bin");
		CleanDirectories("tests/**/obj");
		CleanDirectories("release");
	});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		DotNetCoreRestore(solutionFileName);
		DotNetCoreBuild(solutionFileName, new DotNetCoreBuildSettings { Configuration = configuration, ArgumentCustomization = args => args.Append("--verbosity normal") });
	});

Task("GenerateDocs")
	.IsDependentOn("Build")
	.Does(() => GenerateDocs(verify: false));

Task("VerifyGenerateDocs")
	.IsDependentOn("Build")
	.Does(() => GenerateDocs(verify: true));

Task("Test")
	.IsDependentOn("VerifyGenerateDocs")
	.Does(() =>
	{
		foreach (var projectPath in GetFiles("tests/**/*.csproj").Select(x => x.FullPath))
			DotNetCoreTest(projectPath, new DotNetCoreTestSettings { Configuration = configuration });
	});

Task("NuGetPackage")
	.IsDependentOn("Test")
	.Does(() =>
	{
		foreach (var projectPath in GetFiles("src/**/*.csproj").Select(x => x.FullPath))
			DotNetCorePack(projectPath, new DotNetCorePackSettings { Configuration = configuration, OutputDirectory = "release" });
	});

Task("NuGetPublish")
	.IsDependentOn("NuGetPackage")
	.WithCriteria(() => !string.IsNullOrEmpty(nugetApiKey) && !string.IsNullOrEmpty(githubApiKey))
	.Does(() =>
	{
		var dirtyEntry = gitRepository.RetrieveStatus().FirstOrDefault(x => x.State != FileStatus.Unaltered && x.State != FileStatus.Ignored);
		if (dirtyEntry != null)
			throw new InvalidOperationException($"The git working directory must be clean, but '{dirtyEntry.FilePath}' is dirty.");

		string headSha = gitRepository.Head.Tip.Sha;
		try
		{
			githubClient.Repository.Commit.GetSha1(githubOwner, githubRepo, headSha).GetAwaiter().GetResult();
		}
		catch (Octokit.NotFoundException exception)
		{
			throw new InvalidOperationException($"The current commit '{headSha}' must be pushed to GitHub.", exception);
		}

		string version = null;
		var pushSettings = new NuGetPushSettings { ApiKey = nugetApiKey, Source = nugetSource };
		foreach (var nupkgPath in GetFiles("release/*.nupkg").Select(x => x.FullPath))
		{
			string nupkgVersion = Regex.Match(nupkgPath, @"\.([^\.]+\.[^\.]+\.[^\.]+)\.nupkg$").Groups[1].ToString();
			if (version == null)
				version = nupkgVersion;
			else if (version != nupkgVersion)
				throw new InvalidOperationException($"Mismatched package versions '{version}' and '{nupkgVersion}'.");

			NuGetPush(nupkgPath, pushSettings);
		}

		var tagName = $"nuget-{version}";
		Information($"Creating git tag '{tagName}'...");
		githubClient.Git.Reference.Create(githubOwner, githubRepo,
			new Octokit.NewReference($"refs/tags/{tagName}", headSha)).GetAwaiter().GetResult();
	});

Task("Default")
	.IsDependentOn("Test");

void GenerateDocs(bool verify)
{
	int exitCode = StartProcess($@"cake\XmlDocMarkdown\tools\XmlDocMarkdown.exe",
		$@"{docsAssembly} docs\ --source ""{docsSourceUri}"" --clean" + (verify ? " --verify" : ""));
	if (exitCode == 1 && verify)
		throw new InvalidOperationException("Generated docs don't match; use -target=GenerateDocs to regenerate.");
	else if (exitCode != 0)
		throw new InvalidOperationException($"Docs generation failed with exit code {exitCode}.");
}

void ExecuteProcess(string exePath, string arguments)
{
	int exitCode = StartProcess(exePath, arguments);
	if (exitCode != 0)
		throw new InvalidOperationException($"{exePath} failed with exit code {exitCode}.");
}

RunTarget(target);
