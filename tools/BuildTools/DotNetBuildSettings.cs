namespace BuildTools
{
	public sealed class DotNetBuildSettings
	{
		public string SolutionName { get; set; }

		public string NuGetSource { get; set; }

		public XmlDocOutputSettings XmlDocOutputSettings { get; set; }

		public BuildBotSettings BuildBotSettings { get; set; }

		public string DotNetToolsDirectory { get; set; }
	}
}
