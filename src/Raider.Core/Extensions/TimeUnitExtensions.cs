using Raider.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Raider.Extensions
{
	public static class TimeUnitExtensions
	{
		private static readonly long[,] ConversionFactors = BuildConversionFactorsMatrix();

		private static readonly IReadOnlyDictionary<string, TimeUnit> TimeUnitValueMapping =
			new ReadOnlyDictionary<string, TimeUnit>(
				new Dictionary<string, TimeUnit>
				{
					{ "ns", TimeUnit.Nanoseconds },
					{ "us", TimeUnit.Microseconds },
					{ "ms", TimeUnit.Milliseconds },
					{ "s", TimeUnit.Seconds },
					{ "min", TimeUnit.Minutes },
					{ "h", TimeUnit.Hours },
					{ "days", TimeUnit.Days },
				});

		private static readonly IReadOnlyDictionary<TimeUnit, string> ValueTimeUnitMapping =
			new ReadOnlyDictionary<TimeUnit, string>(
				new Dictionary<TimeUnit, string>
				{
					{ TimeUnit.Nanoseconds, "ns" },
					{ TimeUnit.Microseconds, "us" },
					{ TimeUnit.Milliseconds, "ms" },
					{ TimeUnit.Seconds, "s" },
					{ TimeUnit.Minutes, "min" },
					{ TimeUnit.Hours, "h" },
					{ TimeUnit.Days, "days" },
				});

		public static decimal Convert(this TimeUnit sourceUnit, TimeUnit targetUnit, decimal value)
		{
			if (sourceUnit == targetUnit)
				return value;

			return value * sourceUnit.ScalingFactorFor(targetUnit);
		}

		public static TimeUnit FromUnit(this string unit)
		{
			if (!TimeUnitValueMapping.ContainsKey(unit))
				throw new ArgumentOutOfRangeException(nameof(unit));

			return TimeUnitValueMapping[unit];
		}

		public static decimal ScalingFactorFor(this TimeUnit sourceUnit, TimeUnit targetUnit)
		{
			if (sourceUnit == targetUnit)
				return 1.0m;

			var sourceIndex = (int)sourceUnit;
			var targetIndex = (int)targetUnit;

			if (sourceIndex < targetIndex)
				return 1 / (decimal)ConversionFactors[targetIndex, sourceIndex];

			return ConversionFactors[sourceIndex, targetIndex];
		}

		public static decimal ToDays(this TimeUnit unit, decimal value)
			=> Convert(unit, TimeUnit.Days, value);

		public static decimal ToHours(this TimeUnit unit, decimal value)
			=> Convert(unit, TimeUnit.Hours, value);

		public static decimal ToMicroseconds(this TimeUnit unit, decimal value)
			=> Convert(unit, TimeUnit.Microseconds, value);

		public static decimal ToMilliseconds(this TimeUnit unit, decimal value)
			=> Convert(unit, TimeUnit.Milliseconds, value);

		public static decimal ToMinutes(this TimeUnit unit, decimal value)
			=> Convert(unit, TimeUnit.Minutes, value);

		public static decimal ToNanoseconds(this TimeUnit unit, decimal value)
			=> Convert(unit, TimeUnit.Nanoseconds, value);

		public static decimal ToSeconds(this TimeUnit unit, decimal value)
			=> Convert(unit, TimeUnit.Seconds, value);

		public static TimeSpan ToTimeSpan(this TimeUnit unit, decimal value)
			=> TimeSpan.FromMilliseconds(System.Convert.ToDouble(Convert(unit, TimeUnit.Milliseconds, value)));

		public static string ToUnitString(this TimeUnit unit)
		{
			if (!ValueTimeUnitMapping.ContainsKey(unit))
				throw new ArgumentOutOfRangeException(nameof(unit));

			return ValueTimeUnitMapping[unit];
		}

		private static long[,] BuildConversionFactorsMatrix()
		{
			var count = Enum.GetValues(typeof(TimeUnit)).Length;

			var matrix = new long[count, count];
			var timingFactors = new[]
								{
									1000L, // Nanoseconds to microseconds
									1000L, // Microseconds to milliseconds
									1000L, // Milliseconds to seconds
									60L, // Seconds to minutes
									60L, // Minutes to hours
									24L // Hours to days
								};

			for (var source = 0; source < count; source++)
			{
				var cumulativeFactor = 1L;
				for (var target = source - 1; target >= 0; target--)
				{
					cumulativeFactor *= timingFactors[target];
					matrix[source, target] = cumulativeFactor;
				}
			}

			return matrix;
		}
	}
}
