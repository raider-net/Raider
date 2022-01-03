using Raider.Logging;
using Raider.Exceptions;
using System.Collections.Generic;

namespace Raider
{
	public class Result : IResult
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

		internal Result()
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

			throw new ResultException(this);
		}
	}

	public class Result<T> : Result, IResult<T>, IResult
	{
		public bool DataWasSet { get; private set; }

		private T? _data;
		public T? Data
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
				DataWasSet = true;
			}
		}

		internal Result()
			: base()
		{
		}

		public void ClearData()
		{
			_data = default;
			DataWasSet = false;
		}
	}
}
