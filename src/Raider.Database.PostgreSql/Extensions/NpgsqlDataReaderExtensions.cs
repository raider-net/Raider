using System;
using System.IO;
using System.Threading.Tasks;

namespace Npgsql
{
	public static class NpgsqlDataReaderExtensions
	{
		public static bool? GetNullableBoolean(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (bool?)null : reader.GetBoolean(ordinal);
		}

		public static byte? GetNullableByte(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (byte?)null : reader.GetByte(ordinal);
		}

		public static char? GetNullableChar(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (char?)null : reader.GetChar(ordinal);
		}

		public static DateTime? GetNullableDateTime(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (DateTime?)null : reader.GetDateTime(ordinal);
		}

		public static decimal? GetNullableDecimal(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (decimal?)null : reader.GetDecimal(ordinal);
		}

		public static double? GetNullableDouble(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (double?)null : reader.GetDouble(ordinal);
		}

		public static Guid? GetNullableGuid(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (Guid?)null : reader.GetGuid(ordinal);
		}

		public static short? GetNullableInt16(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (short?)null : reader.GetInt16(ordinal);
		}

		public static int? GetNullableInt32(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (int?)null : reader.GetInt32(ordinal);
		}

		public static long? GetNullableInt64(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (long?)null : reader.GetInt64(ordinal);
		}

		public static float? GetNullableFloat(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (float?)null : reader.GetFloat(ordinal);
		}

		public static Stream? GetNullableStream(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? null : reader.GetStream(ordinal);
		}

		public static async Task<Stream?> GetNullableStreamAsync(this NpgsqlDataReader reader, int ordinal)
		{
			return (await reader.IsDBNullAsync(ordinal)) ? null : (await reader.GetStreamAsync(ordinal));
		}

		public static string? GetNullableString(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
		}

		public static TimeSpan? GetNullableTimeSpan(this NpgsqlDataReader reader, int ordinal)
		{
			return reader.IsDBNull(ordinal) ? (TimeSpan?)null : reader.GetTimeSpan(ordinal);
		}
	}
}
