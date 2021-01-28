using System.Collections.Generic;

namespace Raider.Sql.Metadata
{
	public class DatabaseColumnObject : DatabaseObject
	{
		/// <summary>
		///     The ordered list of columns.
		/// </summary>
		public IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();
	}
}
