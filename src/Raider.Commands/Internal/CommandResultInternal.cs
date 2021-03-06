﻿using Raider.Logging;
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
		public CommandResultInternal()
			: base()
		{
		}

		public TResult? Result { get; set; }
	}
}
