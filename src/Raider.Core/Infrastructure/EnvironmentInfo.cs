using System;
using System.Collections.Generic;

namespace Raider.Infrastructure
{
	public struct EnvironmentInfo : IEnvironmentInfo, Serializer.IDictionaryObject
	{
		public static readonly Guid RUNTIME_UNIQUE_KEY = Guid.NewGuid();

		public static readonly EnvironmentInfo Empty = new EnvironmentInfo(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

		public Guid RuntimeUniqueKey => RUNTIME_UNIQUE_KEY;
		public DateTimeOffset? Created { get; set; }
		public string? RunningEnvironment { get; }
		public string? EntryAssemblyName { get; }
		public string? EntryAssemblyVersion { get; }
		public string? BaseDirectory { get; }
		public string? FrameworkDescription { get; }
		public string? TargetFramework { get; }
		public string? CLRVersion { get; }
		public string? MachineName { get; }
		public string? CurrentAppDomainName { get; }
		public bool? Is64BitOperatingSystem { get; }
		public bool? Is64BitProcess { get; }
		public string? OperatingSystemArchitecture { get; }
		public string? OperatingSystemPlatform { get; }
		public string? OperatingSystemVersion { get; }
		public string? ProcessArchitecture { get; }
		public string? CommandLine { get; }

		public EnvironmentInfo(
			string? runningEnvironment,
			DateTimeOffset? created,
			string? frameworkDescription,
			string? targetFramework,
			string? clrVersion,
			string? entryAssemblyName,
			string? entryAssemblyVersion,
			string? baseDirectory,
			string? machineName,
			string? currentAppDomainName,
			bool? is64BitOperatingSystem,
			bool? is64BitProcess,
			string? operatingSystemPlatform,
			string? operatingSystemVersion,
			string? operatingSystemArchitecture,
			string? processArchitecture,
			string? commandLine)
		{
			RunningEnvironment = runningEnvironment;
			Created = created;
			FrameworkDescription = frameworkDescription;
			TargetFramework = targetFramework;
			CLRVersion = clrVersion;
			EntryAssemblyName = entryAssemblyName;
			EntryAssemblyVersion = entryAssemblyVersion;
			BaseDirectory = baseDirectory;
			MachineName = machineName;
			CurrentAppDomainName = currentAppDomainName;
			Is64BitOperatingSystem = is64BitOperatingSystem;
			Is64BitProcess = is64BitProcess;
			OperatingSystemArchitecture = operatingSystemArchitecture;
			OperatingSystemPlatform = operatingSystemPlatform;
			OperatingSystemVersion = operatingSystemVersion;
			ProcessArchitecture = processArchitecture;
			CommandLine = commandLine;
		}

		public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
			};

			if (!string.IsNullOrWhiteSpace(RunningEnvironment))
				dict.Add(nameof(RunningEnvironment), RunningEnvironment);

			if (Created.HasValue)
				dict.Add(nameof(Created), Created);

			if (!string.IsNullOrWhiteSpace(EntryAssemblyName))
				dict.Add(nameof(EntryAssemblyName), EntryAssemblyName);

			if (!string.IsNullOrWhiteSpace(EntryAssemblyVersion))
				dict.Add(nameof(EntryAssemblyVersion), EntryAssemblyVersion);

			if (!string.IsNullOrWhiteSpace(BaseDirectory))
				dict.Add(nameof(BaseDirectory), BaseDirectory);

			if (!string.IsNullOrWhiteSpace(FrameworkDescription))
				dict.Add(nameof(FrameworkDescription), FrameworkDescription);

			if (!string.IsNullOrWhiteSpace(TargetFramework))
				dict.Add(nameof(TargetFramework), TargetFramework);

			if (!string.IsNullOrWhiteSpace(CLRVersion))
				dict.Add(nameof(CLRVersion), CLRVersion);

			if (!string.IsNullOrWhiteSpace(MachineName))
				dict.Add(nameof(MachineName), MachineName);

			if (!string.IsNullOrWhiteSpace(CurrentAppDomainName))
				dict.Add(nameof(CurrentAppDomainName), CurrentAppDomainName);

			if (Is64BitOperatingSystem.HasValue)
				dict.Add(nameof(Is64BitOperatingSystem), Is64BitOperatingSystem);

			if (Is64BitProcess.HasValue)
				dict.Add(nameof(Is64BitProcess), Is64BitProcess);

			if (!string.IsNullOrWhiteSpace(OperatingSystemArchitecture))
				dict.Add(nameof(OperatingSystemArchitecture), OperatingSystemArchitecture);

			if (!string.IsNullOrWhiteSpace(OperatingSystemPlatform))
				dict.Add(nameof(OperatingSystemPlatform), OperatingSystemPlatform);

			if (!string.IsNullOrWhiteSpace(OperatingSystemVersion))
				dict.Add(nameof(OperatingSystemVersion), OperatingSystemVersion);

			if (!string.IsNullOrWhiteSpace(ProcessArchitecture))
				dict.Add(nameof(ProcessArchitecture), ProcessArchitecture);

			if (!string.IsNullOrWhiteSpace(CommandLine))
				dict.Add(nameof(CommandLine), CommandLine);

			return dict;
		}

		public static bool operator ==(EnvironmentInfo left, EnvironmentInfo right) { return left.Equals(right); }

		public static bool operator !=(EnvironmentInfo left, EnvironmentInfo right) { return !left.Equals(right); }

		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;

			return obj is EnvironmentInfo info && Equals(info);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = FrameworkDescription != null ? FrameworkDescription.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (Created != null ? Created.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (TargetFramework != null ? TargetFramework.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CLRVersion != null ? CLRVersion.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (RunningEnvironment != null ? RunningEnvironment.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (EntryAssemblyName != null ? EntryAssemblyName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (EntryAssemblyVersion != null ? EntryAssemblyVersion.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (BaseDirectory != null ? BaseDirectory.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (MachineName != null ? MachineName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CurrentAppDomainName != null ? CurrentAppDomainName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Is64BitOperatingSystem.HasValue ? Is64BitOperatingSystem.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Is64BitProcess.HasValue ? Is64BitProcess.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (OperatingSystemPlatform != null ? OperatingSystemPlatform.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (OperatingSystemArchitecture != null ? OperatingSystemArchitecture.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (OperatingSystemVersion != null ? OperatingSystemVersion.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ProcessArchitecture != null ? ProcessArchitecture.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CommandLine != null ? CommandLine.GetHashCode() : 0);
				return hashCode;
			}
		}

		public bool Equals(EnvironmentInfo other)
		{
			return
				   string.Equals(FrameworkDescription, other.FrameworkDescription)
				&& string.Equals(Created, other.Created)
				&& string.Equals(TargetFramework, other.TargetFramework)
				&& string.Equals(CLRVersion, other.CLRVersion)
				&& string.Equals(EntryAssemblyName, other.EntryAssemblyName)
				&& string.Equals(RunningEnvironment, other.RunningEnvironment)
				&& string.Equals(EntryAssemblyVersion, other.EntryAssemblyVersion)
				&& string.Equals(BaseDirectory, other.BaseDirectory)
				&& string.Equals(MachineName, other.MachineName)
				&& string.Equals(CurrentAppDomainName, other.CurrentAppDomainName)
				&& string.Equals(Is64BitOperatingSystem, other.Is64BitOperatingSystem)
				&& string.Equals(Is64BitProcess, other.Is64BitProcess)
				&& string.Equals(OperatingSystemPlatform, other.OperatingSystemPlatform)
				&& string.Equals(OperatingSystemArchitecture, other.OperatingSystemArchitecture)
				&& string.Equals(OperatingSystemVersion, other.OperatingSystemVersion)
				&& string.Equals(ProcessArchitecture, other.ProcessArchitecture)
				&& string.Equals(CommandLine, other.CommandLine);
		}
	}
}
