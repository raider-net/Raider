using Raider.Logging;
using System.Collections.Generic;

namespace Raider.Commands
{
	public interface ICommandResult
	{
		List<ILogMessage> SuccessMessages { get; }

		List<ILogMessage> WarningMessages { get; }

		List<IErrorMessage> ErrorMessages { get; }

		bool HasSuccessMessage => 0 < SuccessMessages.Count;

		bool HasWarning => 0 < WarningMessages.Count;

		bool HasError => 0 < ErrorMessages.Count;

		bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

		long? AffectedEntities { get; set; }
	}

	public interface ICommandResult<TResult> : ICommandResult
	{
		TResult? Result { get; set; }
	}
}
