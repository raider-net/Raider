using Raider.Logging;
using System.Collections.Generic;

namespace Raider.Commands
{
	public interface ICommandResult
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

	public interface ICommandResult<TResult> : ICommandResult
	{
		bool ResultWasSet { get; }
		TResult? Result { get; set; }

		void ClearResult();
	}
}
