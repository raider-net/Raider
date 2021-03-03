using Serilog.Core;

namespace Raider.Logging
{
	public static class LoggerSettings
	{
		internal const string LogMessage = "Raider_LogMessage";
		internal const string EnvironmentInfo = "Raider_EnvironmentInfo";
		internal const string HardwareInfo = "Raider_HardwareInfo";

		internal const string LogMessage_Template = "{@Raider_LogMessage}";
		internal const string EnvironmentInfo_Template = "{@Raider_EnvironmentInfo}";
		internal const string HardwareInfo_Template = "{@Raider_HardwareInfo}";

		public static readonly LoggingLevelSwitch LevelSwitch = new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information);
	}
}
