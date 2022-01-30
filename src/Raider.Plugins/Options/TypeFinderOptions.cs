using Raider.Plugins.Catalogs;
using System.Collections.Generic;

namespace Raider.Plugins.Options
{
	public class TypeFinderOptions
	{
		/// <summary>
		/// Gets or sets the <see cref="TypeFinderCriteria"/>
		/// </summary>
		public List<TypeFinderCriteria>? TypeFinderCriterias { get; set; } = new List<TypeFinderCriteria>(new List<TypeFinderCriteria>().AsReadOnly());
	}
}
