using Raider.Trace;
using System;

namespace Raider.Services.Commands
{
	public interface ICommandEntry : Serializer.IDictionaryObject
	{
		Guid IdCommandQueryEntry { get; }
		DateTime Created { get; }
		string CommandQueryName { get; }
		ITraceFrame TraceFrame { get; }
		bool IsQuery { get; }
		string? Data { get; }
		int? IdUser { get; }
		Guid? CorrelationId { get; }
	}
}
