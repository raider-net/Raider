using Raider.Trace;
using System;
using System.Collections.Generic;

namespace Raider.Services.Commands
{
	internal class CommandEntry : ICommandEntry
	{
		public Guid IdCommandQueryEntry { get; }

		public DateTime Created { get; }

		public string CommandQueryName { get; }

		public ITraceFrame TraceFrame { get; }

		public string? Data { get; }

		public bool IsQuery { get; }

		public int? IdUser { get; }

		public Guid? CorrelationId { get; }

		public CommandEntry(string commandName, ITraceInfo traceInfo, string? data)
		{
			if (string.IsNullOrWhiteSpace(commandName))
				throw new ArgumentNullException(nameof(commandName));

			if (traceInfo == null)
				throw new ArgumentNullException(nameof(traceInfo));

			IdCommandQueryEntry = Guid.NewGuid();
			Created = DateTime.Now;
			CommandQueryName = commandName;
			TraceFrame = traceInfo.TraceFrame;
			IsQuery = false;
			Data = data;
			IdUser = traceInfo.IdUser;
			CorrelationId = traceInfo.CorrelationId;
		}

		public IDictionary<string, object?> ToDictionary()
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(IdCommandQueryEntry), IdCommandQueryEntry },
				{ nameof(Created), Created },
				{ nameof(CommandQueryName), CommandQueryName },
				{ nameof(TraceFrame), TraceFrame }
			};

			if (!string.IsNullOrWhiteSpace(Data))
				dict.Add(nameof(Data), Data);

			if (IdUser.HasValue)
				dict.Add(nameof(IdUser), IdUser);

			if (CorrelationId.HasValue)
				dict.Add(nameof(CorrelationId), CorrelationId);

			return dict;
		}
	}
}
