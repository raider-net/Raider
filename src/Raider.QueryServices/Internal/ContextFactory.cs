using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.QueryServices.Queries;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.QueryServices
{
	internal class ContextFactory
	{
		private readonly ServiceFactory _serviceFactory;
		private readonly ILoggerFactory _loggerFactory;

		public ContextFactory(
			ServiceFactory serviceFactory,
			ILoggerFactory loggerFactory)
		{
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		public QueryHandlerContext.Builder<TContext> CreateQueryHandlerContextBuilder<TContext, TBuilder>(
			string? queryName = null,
			Type? handlerType = null,
			ITraceInfo? previousTraceInfo = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<TContext>
		{
			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var traceInfo = new TraceInfoBuilder(traceFrame, previousTraceInfo).Build();
			return CreateQueryHandlerContextBuilder<TContext, TBuilder>(traceInfo, queryName, handlerType);
		}

		public QueryHandlerContext.Builder<TContext> CreateQueryHandlerContextBuilder<TContext, TBuilder>(
			ITraceFrame traceFrame,
			string? queryName = null,
			ITraceInfo? previousTraceInfo = null,
			Type? handlerType = null)
			where TContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<TContext>
		{
			var traceInfo = new TraceInfoBuilder(traceFrame, previousTraceInfo).Build();
			return CreateQueryHandlerContextBuilder<TContext, TBuilder>(traceInfo, queryName, handlerType);
		}

		public QueryHandlerContext.Builder<TContext> CreateQueryHandlerContextBuilder<TContext, TBuilder>(
			ITraceInfo traceInfo,
			string? queryName = null,
			Type? handlerType = null)
			where TContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<TContext>
		{
			if (traceInfo == null)
				throw new ArgumentNullException(nameof(traceInfo));

			var queryHandlerContextBuilder = _serviceFactory.GetRequiredInstance<TBuilder>();

			var applicationContext = _serviceFactory.GetRequiredInstance<IApplicationContext>();

			queryHandlerContextBuilder
				.TraceInfo(traceInfo)
				.ApplicationContext(applicationContext)
				.Logger(_loggerFactory.CreateLogger(handlerType ?? typeof(TContext)))
				.QueryName(queryName);

			return queryHandlerContextBuilder;
		}

		public TQueryServiceContext CreateQueryServiceContext<TQueryService, TQueryServiceContext, THandlerContext, TBuilder>(
			string? queryName = null,
			Type? handlerType = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryServiceContext : QueryServiceContext, new()
			where TQueryService : QueryServiceBase<TQueryServiceContext>
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
		{
			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var tc = _serviceFactory.GetInstance<TraceContext>();

			var traceInfo = new TraceInfoBuilder(traceFrame, tc?.Next()).Build();
			var queryHandlerContextBuilder = CreateQueryHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, queryName, handlerType);
			var queryHandlerContext = queryHandlerContextBuilder.Context;

			var serviceContext = new TQueryServiceContext();
			serviceContext.Init(traceInfo, queryHandlerContext, typeof(TQueryService));
			return serviceContext;
		}

		public TQueryServiceContext CreateQueryServiceContext<THandlerContext, TBuilder, TQueryServiceContext>(
			Type queryServiceType,
			string? queryName = null,
			Type? handlerType = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryServiceContext : QueryServiceContext, new()
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
		{
			if (queryServiceType == null)
				throw new ArgumentNullException(nameof(queryServiceType));

			var queryServiceBaseType = typeof(QueryServiceBase<TQueryServiceContext>);
			if (!queryServiceBaseType.IsAssignableFrom(queryServiceType))
				throw new InvalidOperationException($"queryServiceType {queryServiceType.FullName} must inherit from {queryServiceBaseType.FullName}");

			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var tc = _serviceFactory.GetInstance<TraceContext>();

			var traceInfo = new TraceInfoBuilder(traceFrame, tc?.Next()).Build();
			var queryHandlerContextBuilder = CreateQueryHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, queryName, handlerType);
			var queryHandlerContext = queryHandlerContextBuilder.Context;

			var serviceContext = new TQueryServiceContext();
			serviceContext.Init(traceInfo, queryHandlerContext, queryServiceType);
			return serviceContext;
		}
	}
}
