using System.Data.Common;

namespace Raider.Extensions
{
	public static class DbDataReaderExtensions
	{
		public static T? GetValueOrDefault<T>(this DbDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return reader.IsDBNull(ordinal)
				? default
				: reader.GetFieldValue<T>(ordinal);
		}

		public static T? GetValueOrDefault<T>(this DbDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal)
				? default
				: reader.GetFieldValue<T>(ordinal);
		}

		public static object? GetValueOrNull<T>(this DbDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return reader.IsDBNull(ordinal)
				? (object?)null
				: reader.GetFieldValue<T>(ordinal);
		}

		public static object? GetValueOrNull<T>(this DbDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal)
				? (object?)null
				: reader.GetFieldValue<T>(ordinal);
		}
	}
}
