namespace Raider.MathUtils
{
	public static class MathHelper
	{
		public static string? GetDecimalSeparator()
		{
			if (System.Threading.Thread.CurrentThread != null &&
				System.Threading.Thread.CurrentThread.CurrentCulture != null &&
				System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat != null &&
				!string.IsNullOrWhiteSpace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator))
			{
				return System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			}
			else
			{
				return null;
			}
		}

		public static int? IntParseSafe(string? text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			if (int.TryParse(text, out int value))
				return value;

			return null;
		}

		public static long? LongParseSafe(string? text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			if (long.TryParse(text, out long value))
				return value;

			return null;
		}
	}
}
