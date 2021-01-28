using Raider.Logging;
using System.Collections.Generic;

namespace Raider.Queries.Internal
{
	internal class QueryResultInternal<TResult> : IQueryResult<TResult>
	{
		public List<ILogMessage> SuccessMessages { get; }

		public List<ILogMessage> WarningMessages { get; }

		public List<IErrorMessage> ErrorMessages { get; }

		public long? ResultCount { get; set; }

		public QueryResultInternal()
		{
			SuccessMessages = new List<ILogMessage>();
			WarningMessages = new List<ILogMessage>();
			ErrorMessages = new List<IErrorMessage>();
		}

		public TResult? Result { get; set; }
	}
}
