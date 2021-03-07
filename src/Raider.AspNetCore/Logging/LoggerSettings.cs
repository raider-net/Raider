namespace Raider.AspNetCore.Logging
{
	public static class LoggerSettings
	{
		internal const string Request = "Raider_Request";
		internal const string Response = "Raider_Response";

		internal const string Request_Template = "{@Raider_Request}";
		internal const string Response_Template = "{@Raider_Response}";
	}
}
