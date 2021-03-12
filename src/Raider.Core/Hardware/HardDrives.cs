using Raider.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raider.Hardware
{
	public class HardDrives : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public List<HardDrive> HDDs { get; set; }
		public Dictionary<string, double> LogicalDrivesAvailableFreeSpaceInMB { get; set; }

		public HardDrives()
		{
			HDDs = new List<HardDrive>();
			LogicalDrivesAvailableFreeSpaceInMB = new Dictionary<string, double>();
		}

		public IDictionary<string, object?> ToDictionary()
		{
			var dict = new Dictionary<string, object?>();

			if (HDDs != null)
				for (int i = 0; i < HDDs.Count; i++)
					dict.Add($"{nameof(HDDs)}[{i}]", HDDs[i]?.ToDictionary());

			if (LogicalDrivesAvailableFreeSpaceInMB != null)
			{
				var logicalDrives = LogicalDrivesAvailableFreeSpaceInMB.Keys.OrderBy(x => x).ToList();
				for (int i = 0; i < logicalDrives.Count; i++)
					dict.Add($"{nameof(LogicalDrivesAvailableFreeSpaceInMB)}[{i}]", LogicalDrivesAvailableFreeSpaceInMB[logicalDrives[i]]);
			}

			return dict;
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			sb
				.AppendLineSafe(before);

			if (LogicalDrivesAvailableFreeSpaceInMB != null && 0 < LogicalDrivesAvailableFreeSpaceInMB.Count)
			{
				sb.AppendLine("Available free space:");
				foreach (var kvp in LogicalDrivesAvailableFreeSpaceInMB)
					sb.AppendLine($"{kvp.Key}:\\ {kvp.Value} MB");
				sb.AppendLine();
			}

			foreach (var hdd in HDDs)
				hdd.WriteTo(sb);

			sb.AppendLineSafe(after);
		}
	}
}
