using System.Collections.Generic;

namespace BuildTools
{
	public sealed class XmlDocOutputSettings
	{
		public IReadOnlyList<string> Projects { get; set; }

		public string RepoUrl { get; set; }

		public string SourceUrl { get; set; }
	}
}
