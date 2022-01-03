using Raider.Collections;
using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class RamStick : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public ulong? Capacity { get; set; }
		public uint? ClockSpeed { get; set; }
		public string? Manufacturer { get; set; }
		public string? SerialNumber { get; set; }
		public uint? PositionInRow { get; set; }

		public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
			=> new DictionaryBuilder<string>()
				.AddIfHasValue(nameof(Capacity), Capacity, out _)
				.AddIfHasValue(nameof(ClockSpeed), ClockSpeed, out _)
				.AddIfNotWhiteSpace(nameof(Manufacturer), Manufacturer, out _)
				.AddIfNotWhiteSpace(nameof(SerialNumber), SerialNumber, out _)
				.AddIfHasValue(nameof(PositionInRow), PositionInRow, out _)
				.ToObject();

		public override string ToString()
		{
			return $"{Manufacturer} | SerialNumber = {SerialNumber} | PositionInRow = {PositionInRow}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before)
				.AppendLine($"RamStick = {this}")
				.AppendLineSafe(Capacity.HasValue, () => $"Capacity = {Capacity}")
				.AppendLineSafe(ClockSpeed.HasValue, () => $"ClockSpeed = {ClockSpeed}")
				.AppendLineSafe(after);
		}
	}
}
