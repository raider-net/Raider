//using Raider.DependencyInjection;
//using Raider.QueryServices.Queries;
//using Raider.Trace;
//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;

//namespace Raider.QueryServices
//{
//	internal class NonHandlerQueryServiceFactory : IQueryServiceFactory
//	{
//		private readonly ServiceFactory _serviceFactory;
//		private readonly ContextFactory _contextFactory;

//		public NonHandlerQueryServiceFactory(
//			ServiceFactory serviceFactory,
//			ContextFactory contextFactory)
//		{
//			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
//			_contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
//		}

//		public TQueryService Create<TQueryService, THandlerContext, TBuilder>(
//			string? commandName = null,
//			IEnumerable<MethodParameter>? methodParameters = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//			where TQueryService : QueryServiceBase
//			where THandlerContext : QueryHandlerContext
//			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
//		{
//			var service = _serviceFactory.GetRequiredInstance<TQueryService>();

//			var traceFrame = new TraceFrameBuilder()
//				.CallerMemberName(memberName)
//				.CallerFilePath(sourceFilePath)
//				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
//				.MethodParameters(methodParameters)
//				.Build();

//			var traceInfo = new TraceInfoBuilder(traceFrame, null).Build();
//			var commandHandlerContextBuilder = _contextFactory.CreateQueryHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, commandName, null);
//			var commandHandlerContext = commandHandlerContextBuilder.Context;

//			service.QueryServiceContext = new QueryServiceContext(traceInfo, commandHandlerContext, typeof(TQueryService));
//			return service;
//		}
//	}
//}
