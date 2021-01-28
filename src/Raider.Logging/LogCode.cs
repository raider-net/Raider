namespace Raider.Logging
{
	public enum LogCode : long
	{
		ApplicationStart = 1,
		MethodEntry = 2,
		MethodExit = 3,
		EnvironmentInfo = 4,

		//_FIRST = ApplicationStart,
		//_LAST = EnvironmentInfo
	}
}
