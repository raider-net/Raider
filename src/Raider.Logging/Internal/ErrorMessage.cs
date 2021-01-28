using Raider.Trace;

namespace Raider.Logging.Internal
{
	internal class ErrorMessage : LogMessage, IErrorMessage
	{
		public ErrorMessage(ITraceInfo traceInfo)
			: base(traceInfo)
		{
		}
	}
}
