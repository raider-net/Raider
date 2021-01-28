using System.Data.Common;

namespace Raider.Data.Extensions
{
	public static class DbDataReaderExtensions
	{
		public static T? GetValueOrDefault<T>(this DbDataReader reader, string name)
		{
			var idx = reader.GetOrdinal(name);
			return reader.IsDBNull(idx)
				? default
				: reader.GetFieldValue<T>(idx);
		}

		public static object? GetValueOrNull<T>(this DbDataReader reader, string name)
		{
			var idx = reader.GetOrdinal(name);
			return reader.IsDBNull(idx)
				? (object?)null
				: reader.GetFieldValue<T>(idx);
		}
	}
}
