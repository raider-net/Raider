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
	}

	public interface ICommandResult<TResult> : ICommandResult
	{
		TResult? Result { get; set; }
	}
}
