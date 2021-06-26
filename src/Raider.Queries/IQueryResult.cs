using Raider.Logging;
using System.Collections.Generic;

namespace Raider.Queries
{
	public interface IQueryResult<TResult>
	{
		List<ILogMessage> SuccessMessages { get; }

		List<ILogMessage> WarningMessages { get; }

		List<IErrorMessage> ErrorMessages { get; }

		bool HasSuccessMessage { get; }

		bool HasWarning { get; }

		bool HasError { get; }

		bool HasAnyMessage { get; }

		long? ResultCount { get; set; }

		TResult? Result { get; set; }
	}
}
