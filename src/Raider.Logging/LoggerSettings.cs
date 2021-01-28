using Serilog.Core;

namespace Raider.Logging
{
	public static class LoggerSettings
	{
		public const string FWK_LogMessage = "FWK_LogMessage";
		public const string FWK_EnvironmentInfo = "FWK_EnvironmentInfo";
		public const string FWK_HardwareInfo = "FWK_HardwareInfo";

		public const string FWK_LogMessage_Template = "{@FWK_LogMessage}";
		public const string FWK_EnvironmentInfo_Template = "{@FWK_EnvironmentInfo}";
		public const string FWK_HardwareInfo_Template = "{@FWK_HardwareInfo}";

		public static readonly LoggingLevelSwitch LevelSwitch = new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information);
	}
}
