//using Raider.DependencyInjection;
//using Raider.Services.Commands;
//using Raider.Trace;
//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;

//namespace Raider.Services
//{
//	internal class NonHandlerServiceFactory : IServiceFactory
//	{
//		private readonly ServiceFactory _serviceFactory;
//		private readonly ContextFactory _contextFactory;

//		public NonHandlerServiceFactory(
//			ServiceFactory serviceFactory,
//			ContextFactory contextFactory)
//		{
//			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
//			_contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
//		}

//		public TService Create<TService, THandlerContext, TBuilder>(
//			string? commandName = null,
//			IEnumerable<MethodParameter>? methodParameters = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//			where TService : ServiceBase
//			where THandlerContext : CommandHandlerContext
//			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
//		{
//			var service = _serviceFactory.GetRequiredInstance<TService>();

//			var traceFrame = new TraceFrameBuilder()
//				.CallerMemberName(memberName)
//				.CallerFilePath(sourceFilePath)
//				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
//				.MethodParameters(methodParameters)
//				.Build();

//			var traceInfo = new TraceInfoBuilder(traceFrame, null).Build();
//			var commandHandlerContextBuilder = _contextFactory.CreateCommandHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, commandName, null);
//			var commandHandlerContext = commandHandlerContextBuilder.Context;

//			service.ServiceContext = new ServiceContext(traceInfo, commandHandlerContext, typeof(TService));
//			return service;
//		}
//	}
//}
