//using Raider.QueryServices.Queries;
//using Raider.Trace;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;

//namespace Raider.QueryServices
//{
//	public interface IQueryServiceFactory
//	{
//		TQueryService Create<TQueryService, THandlerContext, TBuilder, TQueryServiceContext>(
//			string? commandName = null,
//			IEnumerable<MethodParameter>? methodParameters = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//			where TQueryServiceContext : QueryServiceContext, new()
//			where TQueryService : QueryServiceBase<TQueryServiceContext>
//			where THandlerContext : QueryHandlerContext
//			where TBuilder : QueryHandlerContext.Builder<THandlerContext>;
//	}
//}
