using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#if !NET452
using System.Runtime.InteropServices;
using System.Text;
#endif

namespace Raider.Infrastructure
{
	internal class EnvironmentInfoProviderCache
	{
		public static EnvironmentInfoProviderCache Instance { get; } = new EnvironmentInfoProviderCache();

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
		public string? RunningEnvironment { get; }
		public string? CommandLine { get; }

		private EnvironmentInfoProviderCache()
		{
			ProcessArchitecture = GetSafeString(GetProcessArchitecture);
			OperatingSystemVersion = GetSafeString(GetOSVersion);
			OperatingSystemPlatform = GetSafeString(GetOSPlatform);
			OperatingSystemArchitecture = GetSafeString(GetOSArchitecture);
			MachineName = GetSafeString(() => Environment.MachineName);
			Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
			Is64BitProcess = Environment.Is64BitProcess;
			FrameworkDescription = GetSafeString(GetFrameworkDescription);
			CLRVersion = Environment.Version?.ToString();
			CommandLine = GetSafeString(GetCommandLine);

			var aspnetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

			if (!string.IsNullOrWhiteSpace(aspnetCoreEnv))
			{
				RunningEnvironment = aspnetCoreEnv.ToLowerInvariant();
			}
			else
			{
#if DEBUG
				RunningEnvironment = "debug";
#elif RELEASE
				RunningEnvironment = "release";
#else
				RunningEnvironment = "unknown";
#endif
			}

			var entryAssembly = Assembly.GetEntryAssembly();
			EntryAssemblyName = GetSafeString(() => entryAssembly?.GetName().Name ?? "unknown");
			EntryAssemblyVersion = GetSafeString(() => entryAssembly?.GetName()?.Version?.ToString() ?? "unknown");
			BaseDirectory = AppContext.BaseDirectory;

			var frameworkAttribute = entryAssembly?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();
			TargetFramework = frameworkAttribute?.FrameworkName;

			try
			{
				CurrentAppDomainName = AppDomain.CurrentDomain.FriendlyName;
			}
			catch
			{
				CurrentAppDomainName = "-";
			}

		}

#if (NETSTANDARD2_0 || NETSTANDARD2_1 || NET5_0_OR_GREATER)
		private static string GetOSPlatform()
		{
			var platform = OSPlatform.Create("Other Platform");
			var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
			platform = isWindows ? OSPlatform.Windows : platform;
			var isOsx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
			platform = isOsx ? OSPlatform.OSX : platform;
			var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
			platform = isLinux ? OSPlatform.Linux : platform;
			return platform.ToString();
		}
#else
		private static string GetOSPlatform() { return Environment.OSVersion.Platform.ToString(); }
#endif

#if (NETSTANDARD2_0 || NETSTANDARD2_1 || NET5_0_OR_GREATER)
		private static string GetProcessArchitecture()
		{
			return RuntimeInformation.ProcessArchitecture.ToString();
		}
#else
		private static string GetProcessArchitecture() { return Environment.Is64BitProcess ? "X64" : "X86"; }
#endif

#if (NETSTANDARD2_0 || NETSTANDARD2_1 || NET5_0_OR_GREATER)
		private static string GetOSArchitecture()
		{
			return RuntimeInformation.OSArchitecture.ToString();
		}
#else
		private static string GetOSArchitecture() { return Environment.Is64BitOperatingSystem ? "X64" : "X86"; }
#endif

#if (NETSTANDARD2_0 || NETSTANDARD2_1 || NET5_0_OR_GREATER)
		private static string GetOSVersion()
		{
			return RuntimeInformation.OSDescription;
		}
#else
		private static string GetOSVersion()
		{
			// DEVNOTE: Not ideal but it's a pain to get detect the actual OS Version
			return Environment.OSVersion.VersionString;
		}
#endif

#if (NETSTANDARD2_0 || NETSTANDARD2_1 || NET5_0_OR_GREATER)
		private static string GetFrameworkDescription()
		{
			return RuntimeInformation.FrameworkDescription;
		}
#else
		private static string GetFrameworkDescription() { return AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName; }
#endif

		private static string GetCommandLine()
		{
			var commandLine = GetSafeString(() => Environment.CommandLine);
			var commandLineArgs = Environment.GetCommandLineArgs();

			var sb = new StringBuilder();

			var empty = true;
			if (!string.IsNullOrWhiteSpace(commandLine))
			{
				sb.Append(commandLine);
				empty = false;
			}

			if (0 < commandLineArgs?.Length)
			{
				var firstArg = GetSafeString(() => commandLineArgs[0]);
				if (!string.IsNullOrWhiteSpace(firstArg) && commandLine != firstArg)
				{
					if (empty)
						sb.Append(firstArg);
					else
						sb.Append($" {firstArg}");

					empty = false;
				}

				foreach (var arg in commandLineArgs.Skip(1))
				{
					if (empty)
						sb.Append(arg);
					else
						sb.Append($" {arg}");

					empty = false;
				}
			}

			return sb.ToString();
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		internal static string GetSafeString(Func<string> action)
		{
			try
			{
				return action?.Invoke()?.Trim() ?? string.Empty;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}
}
