using Raider.Services.Commands;
using Raider.Trace;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.Services
{
	public interface IServiceFactory
	{
		TService Create<TService, TServiceContext, THandlerContext, TBuilder>(
			string? commandName = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TServiceContext : ServiceContext, new()
			where TService : ServiceBase<TServiceContext>
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>;
	}
}
