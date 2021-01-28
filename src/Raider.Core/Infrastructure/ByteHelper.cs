using System;

namespace Raider.Infrastructure
{
	public static class ByteHelper
	{
		private const double MB = (double)(1024 * 1024);
		private const double GB = (double)(1024 * 1024 * 1024);

		public static double ConvertToRoundedMegaBytes(ulong bytes)
			=> Math.Round(ConvertToMegaBytes(bytes), 2);

		public static double ConvertToMegaBytes(ulong bytes)
			=> bytes / MB;

		public static double? ConvertToRoundedMegaBytes(ulong? bytes)
			=> bytes.HasValue
				? Math.Round(ConvertToMegaBytes(bytes.Value), 2)
				: (double?)null;

		public static double? ConvertToMegaBytes(ulong? bytes)
			=> bytes.HasValue
				? bytes.Value / MB
				: (double?)null;

		public static double ConvertToRoundedGigaBytes(ulong bytes)
			=> Math.Round(ConvertToGigaBytes(bytes), 2);

		public static double ConvertToGigaBytes(ulong bytes)
			=> bytes / GB;

		public static double? ConvertToRoundedGigaBytes(ulong? bytes)
			=> bytes.HasValue
				? Math.Round(ConvertToGigaBytes(bytes.Value), 2)
				: (double?)null;

		public static double? ConvertToGigaBytes(ulong? bytes)
			=> bytes.HasValue
				? bytes.Value / GB
				: (double?)null;
	}
}
