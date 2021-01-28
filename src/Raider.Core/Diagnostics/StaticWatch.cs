using System;
using System.Diagnostics;

namespace Raider.Diagnostics
{
	/*
	 USAGE:
		var startTicks = StaticWatch.CurrentTicks;

		...some actions...

		var elapsedMilliseconds = StaticWatch.ElapsedMilliseconds(startTicks);

	OR 

		var startTicks = StaticWatch.CurrentTicks;
		
		...some actions...
	
		var endTicks = StaticWatch.CurrentTicks;
		var elapsedTicks = StaticWatch.ElapsedTimeStamp(startTicks, endTicks);            --->>> output: 00:00:00.4096816
		var elapsedTicks = StaticWatch.ElapsedTicks(startTicks, endTicks);                --->>> output: 4096816
		var elapsedMilliseconds = StaticWatch.ElapsedMilliseconds(startTicks, endTicks);  --->>> output: 409,6816
		var elapsedSeconds = StaticWatch.ElapsedSeconds(startTimeStamp, endTimeStamp);    --->>> output: 0,4096816
		var elapsedSeconds = Raider.TimeUnit.Milliseconds.ToSeconds(elapsedMilliseconds);    --->>> output: 0,4096816
	 */

	public static class StaticWatch
	{
		//private static readonly long Factor = 1000L * 1000L * 1000L / Stopwatch.Frequency;
		//public static long Nanoseconds => Stopwatch.GetTimestamp() * Factor;

		private static readonly long TicksPerMillisecond = 10000;
		private static readonly long TicksPerSecond = TicksPerMillisecond * 1000;

		private static readonly double tickFrequency =
			Stopwatch.IsHighResolution
				? TicksPerSecond / Stopwatch.Frequency
				: 1;

		public static long CurrentTicks => Stopwatch.GetTimestamp();

		public static long ElapsedTicks(long startTicks)
			=> ElapsedTicks(startTicks, Stopwatch.GetTimestamp());

		public static long ElapsedTicks(long startTicks, long endTicks)
		{
			long elapsedTicks = endTicks - startTicks;

			if (elapsedTicks < 0)
			{
				// When measuring small time periods the StopWatch.Elapsed* 
				// properties can return negative values.  This is due to 
				// bugs in the basic input/output system (BIOS) or the hardware
				// abstraction layer (HAL) on machines with variable-speed CPUs
				// (e.g. Intel SpeedStep).

				elapsedTicks = 0;
			}

			return elapsedTicks;
		}

		public static long ElapsedDateTimeTicks(long startTicks)
		{
			long elapsedTicks = ElapsedTicks(startTicks);
			return GetElapsedDateTimeTicksInternal(elapsedTicks);
		}

		public static long ElapsedDateTimeTicks(long startTicks, long endTicks)
		{
			long elapsedTicks = ElapsedTicks(startTicks, endTicks);
			return GetElapsedDateTimeTicksInternal(elapsedTicks);
		}

		public static decimal ElapsedMilliseconds(long startTicks)
		{
			long elapsedTicks = ElapsedTicks(startTicks);
			return GetElapsedDateTimeTicksInternal(elapsedTicks) / (decimal)TicksPerMillisecond;
		}

		public static decimal ElapsedMilliseconds(long startTicks, long endTicks)
		{
			long elapsedTicks = ElapsedTicks(startTicks, endTicks);
			return GetElapsedDateTimeTicksInternal(elapsedTicks) / (decimal)TicksPerMillisecond;
		}

		public static decimal ElapsedSeconds(long startTicks)
		{
			long elapsedTicks = ElapsedTicks(startTicks);
			return GetElapsedDateTimeTicksInternal(elapsedTicks) / (decimal)TicksPerSecond;
		}

		public static decimal ElapsedSeconds(long startTicks, long endTicks)
		{
			long elapsedTicks = ElapsedTicks(startTicks, endTicks);
			return GetElapsedDateTimeTicksInternal(elapsedTicks) / (decimal)TicksPerSecond;
		}

		public static TimeSpan ElapsedTimeStamp(long startTicks)
		{
			long elapsedTicks = ElapsedTicks(startTicks);
			return new TimeSpan(GetElapsedDateTimeTicksInternal(elapsedTicks));
		}

		public static TimeSpan ElapsedTimeStamp(long startTicks, long endTicks)
		{
			long elapsedTicks = ElapsedTicks(startTicks, endTicks);
			return new TimeSpan(GetElapsedDateTimeTicksInternal(elapsedTicks));
		}

		// Get the elapsed ticks.
		private static long GetElapsedDateTimeTicksInternal(long elapsedTicks)
		{
			if (Stopwatch.IsHighResolution)
			{
				// convert high resolution perf counter to DateTime ticks
				double dticks = elapsedTicks;
				dticks *= tickFrequency;
				return unchecked((long)dticks);
			}
			else
			{
				return elapsedTicks;
			}
		}
	}
}
