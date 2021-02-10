using Raider.Logging;
using Raider.Queries;
using System.Collections.Generic;

namespace Raider.QueryServices.Queries
{
	public class QueryResult<T> : IQueryResult<T>
	{
		//public ITraceInfo TraceInfo { get; set; }

		public List<ILogMessage> SuccessMessages { get; }

		public List<ILogMessage> WarningMessages { get; }

		public List<IErrorMessage> ErrorMessages { get; }

		public bool HasSuccessMessage => 0 < SuccessMessages.Count;

		public bool HasWarning => 0 < WarningMessages.Count;

		public bool HasError => 0 < ErrorMessages.Count;

		public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

		public long? ResultCount { get; set; }

		public T? Result { get; set; }

		internal QueryResult()
		{
			//TraceInfo = traceInfo;
			SuccessMessages = new List<ILogMessage>();
			WarningMessages = new List<ILogMessage>();
			ErrorMessages = new List<IErrorMessage>();
		}
	}
}
