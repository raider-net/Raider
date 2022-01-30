using Raider.Reflection.Loader;
using System;
using System.Collections.Generic;

namespace Raider.Plugins.Catalogs
{
	public class TypeFinderCriteria
	{
		public Type? Inherits { get; set; }
		public Type? Implements { get; set; }
		public Type? AssignableTo { get; set; }
		public bool? IsAbstract { get; set; }
		public bool? IsInterface { get; set; }
		public string? Name { get; set; }
		public Func<ITypeFindingContext, Type, bool>? Query { get; set; }
		public Type? HasAttribute { get; set; }
		public List<string>? Tags { get; set; } = new List<string>();
	}
}
