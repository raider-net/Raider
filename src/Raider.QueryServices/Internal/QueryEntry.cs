using Raider.Trace;
using System;
using System.Collections.Generic;

namespace Raider.QueryServices.Queries
{
	internal class QueryEntry : IQueryEntry
	{
		public Guid IdCommandQueryEntry { get; }

		public DateTime Created { get; }

		public string CommandQueryName { get; }

		public ITraceFrame TraceFrame { get; }

		public bool IsQuery { get; }

		public string? Data { get; }

		public int? IdUser { get; }

		public Guid? CorrelationId { get; }

		public QueryEntry(string queryName, ITraceInfo traceInfo, string? data)
		{
			if (string.IsNullOrWhiteSpace(queryName))
				throw new ArgumentNullException(nameof(queryName));

			if (traceInfo == null)
				throw new ArgumentNullException(nameof(traceInfo));

			IdCommandQueryEntry = Guid.NewGuid();
			Created = DateTime.Now;
			CommandQueryName = queryName;
			TraceFrame = traceInfo.TraceFrame;
			IsQuery = true;
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
				{ nameof(TraceFrame), TraceFrame.ToString() },
				{ nameof(IsQuery), IsQuery }
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
