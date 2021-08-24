using Raider.Logging;
using Raider.Queries;
using Raider.Queries.Exceptions;
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

		public bool ResultWasSet { get; private set; }

		private T? _result;
		public T? Result
		{
			get
			{
				return _result;
			}
			set
			{
				_result = value;
				ResultWasSet = true;
			}
		}

		internal QueryResult()
		{
			//TraceInfo = traceInfo;
			SuccessMessages = new List<ILogMessage>();
			WarningMessages = new List<ILogMessage>();
			ErrorMessages = new List<IErrorMessage>();
		}

		public void ClearResult()
		{
			_result = default;
			ResultWasSet = false;
		}

		public void ThrowIfError()
		{
			if (!HasError)
				return;

			throw new QueryResultException<T>(this);
		}
	}
}
