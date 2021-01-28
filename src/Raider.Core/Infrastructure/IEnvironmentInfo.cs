using System;

namespace Raider.Infrastructure
{
	public interface IEnvironmentInfo : Serializer.IDictionaryObject
	{
		Guid RuntimeUniqueKey { get; }

		string? RunningEnvironment { get; }

		string? EntryAssemblyName { get; }

		string? EntryAssemblyVersion { get; }

		string? BaseDirectory { get; }

		string? FrameworkDescription { get; }

		string? TargetFramework { get; }

		string? CLRVersion { get; }

		string? MachineName { get; }

		string? CurrentAppDomainName { get; }

		bool? Is64BitOperatingSystem { get; }

		bool? Is64BitProcess { get; }

		string? OperatingSystemArchitecture { get; }

		string? OperatingSystemPlatform { get; }

		string? OperatingSystemVersion { get; }

		string? ProcessArchitecture { get; }

		string? CommandLine { get; }
	}
}
