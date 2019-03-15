using System.Collections.Generic;

namespace BuildTools
{
	public sealed class BuildTarget
	{
		public string Name { get; }

		public IReadOnlyList<string> Dependencies { get; }

		internal BuildTarget(string name, IReadOnlyList<string> dependencies)
		{
			Name = name;
			Dependencies = dependencies;
		}
	}
}
