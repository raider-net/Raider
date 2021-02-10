using Raider.Trace;

namespace Raider.Logging
{
	public class ErrorMessage : LogMessage, IErrorMessage
	{
		internal ErrorMessage(ITraceInfo traceInfo)
			: base(traceInfo)
		{
		}
	}
}
