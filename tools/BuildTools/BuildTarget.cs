using System.Collections.Generic;

namespace BuildTools
{
	public sealed class BuildTarget
	{
		public string Name { get; }

		public string Description { get; }

		public IReadOnlyList<string> Dependencies { get; }

		internal BuildTarget(string name, string description, IReadOnlyList<string> dependencies)
		{
			Name = name;
			Description = description;
			Dependencies = dependencies;
		}
	}
}
