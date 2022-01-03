using Raider.Collections;
using Raider.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Raider.Hardware
{
	public class OS : Serializer.IDictionaryObject, Serializer.ITextSerializer
	{
		public string? MachineName { get; set; }
		public string? Caption { get; set; }
		public string? OSArchitecture { get; set; }
		public string? Version { get; set; }
		public string? BuildNumber { get; set; }
		public string? SerialNumber { get; set; }
		public string? SystemDrive { get; set; }
		public string? CodeSet { get; set; }
		public uint? ProductType { get; set; }

		public string? SubVersion { get; private set; }
		public bool IsWorkstation => ProductType == 1;
		public bool IsDomainController => ProductType == 2;
		public bool IsServer => ProductType == 3;

		public OS SetSubVersion()
		{
			if (string.IsNullOrWhiteSpace(BuildNumber) || !ProductType.HasValue)
				return this;

			if (IsWorkstation)
			{
				if (BuildNumber == "7601")
					SubVersion = "Service Pack 1"; //Windows 7
				else if (BuildNumber == "10240")
					SubVersion = "Version 1507"; //Windows 10
				else if (BuildNumber == "10586")
					SubVersion = "Version 1511"; //Windows 10
				else if (BuildNumber == "14393")
					SubVersion = "Version 1607"; //Windows 10
				else if (BuildNumber == "15063")
					SubVersion = "Version 1703"; //Windows 10
				else if (BuildNumber == "16299")
					SubVersion = "Version 1709"; //Windows 10
				else if (BuildNumber == "17134")
					SubVersion = "Version 1803"; //Windows 10
				else if (BuildNumber == "17763")
					SubVersion = "Version 1809"; //Windows 10
				else if (BuildNumber == "18362")
					SubVersion = "Version 1903"; //Windows 10
				else if (BuildNumber == "18363")
					SubVersion = "Version 1909"; //Windows 10
			}
			else if (IsServer)
			{
				if (BuildNumber == "7601")
					SubVersion = "Service Pack 1"; //Windows Server 2008 R2
				else if (BuildNumber == "14393")
					SubVersion = "Version 1607"; //Windows Server 2016
				else if (BuildNumber == "16299")
					SubVersion = "Version 1709"; //Windows Server 2016
				else if (BuildNumber == "17763")
					SubVersion = "Version 1809"; //Windows Server 2016
			}

			return this;
		}

		public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
			=> new DictionaryBuilder<string>()
				.AddIfNotWhiteSpace(nameof(MachineName), MachineName, out _)
				.AddIfNotWhiteSpace(nameof(Caption), Caption, out _)
				.AddIfNotWhiteSpace(nameof(OSArchitecture), OSArchitecture, out _)
				.AddIfNotWhiteSpace(nameof(Version), Version, out _)
				.AddIfNotWhiteSpace(nameof(BuildNumber), BuildNumber, out _)
				.AddIfNotWhiteSpace(nameof(SerialNumber), SerialNumber, out _)
				.AddIfNotWhiteSpace(nameof(SystemDrive), SystemDrive, out _)
				.AddIfNotWhiteSpace(nameof(CodeSet), CodeSet, out _)
				.AddIfHasValue(nameof(ProductType), ProductType, out _)
				.AddIfNotWhiteSpace(nameof(SubVersion), SubVersion, out _)
				.ToObject();

		public override string ToString()
		{
			var subVer = string.IsNullOrWhiteSpace(SubVersion)
				? ""
				: $", {SubVersion}";

			return $"{Caption}{subVer} | {Version} | {OSArchitecture} | {MachineName} | {CodeSet}";
		}

		public void WriteTo(StringBuilder sb, string? before = null, string? after = null)
		{
			var subVer = string.IsNullOrWhiteSpace(SubVersion)
				? ""
				: $", {SubVersion}";

			sb
				.AppendLineSafe(before)
				.AppendLine($"Device name = {MachineName}")
				.AppendLine($"Windows edition = {Caption}{subVer}, OS build {BuildNumber}")
				.AppendLine($"Architecture = {OSArchitecture}")
				.AppendLine($"Serial number = {SerialNumber}")
				.AppendLine($"CodeSet = {CodeSet}")
				.AppendLine($"SystemDrive = {SystemDrive}")
				.AppendLineSafe(after);
		}
	}
}
