using System.Collections.Generic;

namespace Raider.Hardware.Options
{
	public class HWGathererOptions
	{
		public static HWGathererOptions HWGathererOptionsForLogger => new HWGathererOptions
		{
			GetProcessorUsage = true,
			LoadMemoryInfo = true,
			GetMemoryUsage = true,
			LoadBiosInfo = false,
			LoadGraphicsCards = false,
			LoadHardDrives = true,
			LogicalDrives = new List<string> { "C" },
			LoadMotherBoardInfo = false,
			LoadNetworkAdapters = false,
			LoadOSInfo = true,
			LoadSecurityProducts = false,
			LoadUserAccounts = false
		};
		public static HWGathererOptions HWGathererOptionsAll => new HWGathererOptions
		{
			GetProcessorUsage = true,
			LoadMemoryInfo = true,
			GetMemoryUsage = true,
			LoadBiosInfo = true,
			LoadGraphicsCards = true,
			LoadHardDrives = true,
			LogicalDrives = new List<string> { "C" },
			LoadMotherBoardInfo = true,
			LoadNetworkAdapters = true,
			LoadOSInfo = true,
			LoadSecurityProducts = true,
			LoadUserAccounts = true
		};

		public bool GetProcessorUsage { get; set; }
		public bool LoadMemoryInfo { get; set; }
		public bool GetMemoryUsage { get; set; }
		public bool LoadBiosInfo { get; set; }
		public bool LoadGraphicsCards { get; set; }
		public bool LoadHardDrives { get; set; }
		public List<string> LogicalDrives { get; set; } // = new List<string> { "C" };
		public bool LoadMotherBoardInfo { get; set; }
		public bool LoadNetworkAdapters { get; set; }
		public bool LoadOSInfo { get; set; }
		public bool LoadSecurityProducts { get; set; }
		public bool LoadUserAccounts { get; set; }
	}
}
