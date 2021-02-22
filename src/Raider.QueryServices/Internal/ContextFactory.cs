﻿using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.Localization;
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
		private readonly IApplicationContext _applicationContext;
		private readonly IApplicationResources _applicationResources;
		private readonly ILoggerFactory _loggerFactory;

		public ContextFactory(
			ServiceFactory serviceFactory,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources,
			ILoggerFactory loggerFactory)
		{
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			_applicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
			_applicationResources = applicationResources ?? throw new ArgumentNullException(nameof(applicationResources));
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

			queryHandlerContextBuilder
				.TraceInfo(traceInfo)
				.Principal(_applicationContext.Principal)
				.User(_applicationContext.User)
				.Logger(_loggerFactory.CreateLogger(handlerType ?? typeof(TContext)))
				.ApplicationResources(_applicationResources)
				.QueryName(queryName);

			return queryHandlerContextBuilder;
		}

		public QueryServiceContext CreateQueryServiceContext<TQueryService, THandlerContext, TBuilder>(
			string? queryName = null,
			Type? handlerType = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TQueryService : QueryServiceBase
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
		{
			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var traceInfo = new TraceInfoBuilder(traceFrame, null).Build();
			var queryHandlerContextBuilder = CreateQueryHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, queryName, handlerType);
			var queryHandlerContext = queryHandlerContextBuilder.Context;

			var serviceContext = new QueryServiceContext(traceInfo, queryHandlerContext, typeof(TQueryService));
			return serviceContext;
		}

		public QueryServiceContext CreateQueryServiceContext<THandlerContext, TBuilder>(
			Type queryServiceType,
			string? queryName = null,
			Type? handlerType = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
		{
			if (queryServiceType == null)
				throw new ArgumentNullException(nameof(queryServiceType));

			var queryServiceBaseType = typeof(QueryServiceBase);
			if (!queryServiceBaseType.IsAssignableFrom(queryServiceType))
				throw new InvalidOperationException($"queryServiceType {queryServiceType.FullName} must inherit from {queryServiceBaseType.FullName}");

			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var traceInfo = new TraceInfoBuilder(traceFrame, null).Build();
			var queryHandlerContextBuilder = CreateQueryHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, queryName, handlerType);
			var queryHandlerContext = queryHandlerContextBuilder.Context;

			var serviceContext = new QueryServiceContext(traceInfo, queryHandlerContext, queryServiceType);
			return serviceContext;
		}
	}
}