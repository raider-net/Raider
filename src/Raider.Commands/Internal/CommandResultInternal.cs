using Raider.Logging;
using System.Collections.Generic;

namespace Raider.Commands.Internal
{
	internal class CommandResultInternal : ICommandResult
	{
		public List<ILogMessage> SuccessMessages { get; }

		public List<ILogMessage> WarningMessages { get; }

		public List<IErrorMessage> ErrorMessages { get; }

		public bool HasSuccessMessage => 0 < SuccessMessages.Count;

		public bool HasWarning => 0 < WarningMessages.Count;

		public bool HasError => 0 < ErrorMessages.Count;

		public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

		public long? AffectedEntities { get; set; }

		public CommandResultInternal()
		{
			SuccessMessages = new List<ILogMessage>();
			WarningMessages = new List<ILogMessage>();
			ErrorMessages = new List<IErrorMessage>();
		}
	}

	internal class CommandResultInternal<TResult> : CommandResultInternal, ICommandResult<TResult>
	{
		public bool ResultWasSet { get; private set; }

		private TResult? _result;
		public TResult? Result
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

		public CommandResultInternal()
			: base()
		{
		}

		public void ClearResult()
		{
			_result = default;
			ResultWasSet = false;
		}
	}
}
