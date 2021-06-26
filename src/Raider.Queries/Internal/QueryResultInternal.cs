using Raider.Logging;
using System.Collections.Generic;

namespace Raider.Queries.Internal
{
	internal class QueryResultInternal<TResult> : IQueryResult<TResult>
	{
		public List<ILogMessage> SuccessMessages { get; }

		public List<ILogMessage> WarningMessages { get; }

		public List<IErrorMessage> ErrorMessages { get; }

		public bool HasSuccessMessage => 0 < SuccessMessages.Count;

		public bool HasWarning => 0 < WarningMessages.Count;

		public bool HasError => 0 < ErrorMessages.Count;

		public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

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
