using Raider.QueryServices.Queries;
using Raider.Trace;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.QueryServices
{
	public interface IQueryServiceFactory
	{
		TQueryService Create<TQueryService, THandlerContext, TBuilder>(
			string? commandName = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryService : QueryServiceBase
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>;
	}
}
