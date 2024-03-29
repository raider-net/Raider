﻿using Raider.Commands;
using Raider.Commands.Exceptions;
using Raider.Logging;
using System.Collections.Generic;

namespace Raider.Services.Commands
{
	public class CommandResult : ICommandResult
	{
		//public ITraceInfo TraceInfo { get; set; }

		public List<ILogMessage> SuccessMessages { get; }

		public List<ILogMessage> WarningMessages { get; }

		public List<IErrorMessage> ErrorMessages { get; }

		public bool HasSuccessMessage => 0 < SuccessMessages.Count;

		public bool HasWarning => 0 < WarningMessages.Count;

		public bool HasError => 0 < ErrorMessages.Count;

		public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

		public long? AffectedEntities { get; set; }

		internal CommandResult()
		{
			//TraceInfo = traceInfo;
			SuccessMessages = new List<ILogMessage>();
			WarningMessages = new List<ILogMessage>();
			ErrorMessages = new List<IErrorMessage>();
		}

		public void ThrowIfError()
		{
			if (!HasError)
				return;

			throw new CommandResultException(this);
		}
	}

	public class CommandResult<T> : CommandResult, ICommandResult<T>, ICommandResult
	{
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

		internal CommandResult()
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
