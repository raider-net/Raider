using Raider.Logging;
using System.Collections.Generic;

namespace Raider
{
	public interface IResult
	{
		List<ILogMessage> SuccessMessages { get; }

		List<ILogMessage> WarningMessages { get; }

		List<IErrorMessage> ErrorMessages { get; }

		bool HasSuccessMessage { get; }

		bool HasWarning { get; }

		bool HasError { get; }

		bool HasAnyMessage { get; }

		long? AffectedEntities { get; set; }

		void ThrowIfError();
	}

	public interface IResult<TData> : IResult
	{
		bool DataWasSet { get; }
		TData? Data { get; set; }

		void ClearData();
	}
}
