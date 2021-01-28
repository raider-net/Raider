namespace Raider.Diagnostics
{
	public static class StackTraceHelper
	{
		public static string GetStackTrace(bool captureFileNameAndLineNumber)
			=> GetStackTrace(1, captureFileNameAndLineNumber);

		public static string GetStackTrace(int skipFrames, bool captureFileNameAndLineNumber)
			=> new System.Diagnostics.StackTrace(skipFrames, captureFileNameAndLineNumber).ToString();
	}
}
