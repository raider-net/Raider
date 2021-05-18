using System;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Extensions
{
	public static class DateTimeExtensions
	{
		public static DateTime ToEndOfDay(this DateTime dateTime)
			=> dateTime.Date.AddDays(1).AddMilliseconds(-1);

		[return: NotNullIfNotNull("dateTime")]
		public static DateTime? ToEndOfDay(this DateTime? dateTime)
			=> dateTime?.Date.AddDays(1).AddMilliseconds(-1);
	}
}
