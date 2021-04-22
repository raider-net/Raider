using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.Services.Commands;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.Services
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

		public CommandHandlerContext.Builder<TContext> CreateCommandHandlerContextBuilder<TContext, TBuilder>(
			string? commandName = null,
			Type? handlerType = null,
			ITraceInfo? previousTraceInfo = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<TContext>
		{
			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var traceInfo = new TraceInfoBuilder(traceFrame, previousTraceInfo).Build();
			return CreateCommandHandlerContextBuilder<TContext, TBuilder>(traceInfo, commandName, handlerType);
		}

		public CommandHandlerContext.Builder<TContext> CreateCommandHandlerContextBuilder<TContext, TBuilder>(
			ITraceFrame traceFrame,
			string? commandName = null,
			ITraceInfo? previousTraceInfo = null,
			Type? handlerType = null)
			where TContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<TContext>
		{
			var traceInfo = new TraceInfoBuilder(traceFrame, previousTraceInfo).Build();
			return CreateCommandHandlerContextBuilder<TContext, TBuilder>(traceInfo, commandName, handlerType);
		}

		public CommandHandlerContext.Builder<TContext> CreateCommandHandlerContextBuilder<TContext, TBuilder>(
			ITraceInfo traceInfo,
			string? commandName = null,
			Type? handlerType = null)
			where TContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<TContext>
		{
			if (traceInfo == null)
				throw new ArgumentNullException(nameof(traceInfo));

			var commandHandlerContextBuilder = _serviceFactory.GetRequiredInstance<TBuilder>();

			var applicationContext = _serviceFactory.GetRequiredInstance<IApplicationContext>();

			commandHandlerContextBuilder
				.TraceInfo(traceInfo)
				.ApplicationContext(applicationContext)
				.Logger(_loggerFactory.CreateLogger(handlerType ?? typeof(TContext)))
				.CommandName(commandName);

			return commandHandlerContextBuilder;
		}

		public TServiceContext CreateServiceContext<TService, TServiceContext, THandlerContext, TBuilder>(
			string? commandName = null,
			Type? handlerType = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TServiceContext : ServiceContext, new()
			where TService : ServiceBase<TServiceContext>
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
		{
			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var appCtx = _serviceFactory.GetRequiredInstance<IApplicationContext>();

			var traceInfo = new TraceInfoBuilder(traceFrame, appCtx.Next()).Build();
			var commandHandlerContextBuilder = CreateCommandHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, commandName, handlerType);
			var commandHandlerContext = commandHandlerContextBuilder.Context;

			var serviceContext = new TServiceContext();
			serviceContext.Init(traceInfo, commandHandlerContext, typeof(TService));
			return serviceContext;
		}

		public TServiceContext CreateServiceContext<THandlerContext, TBuilder, TServiceContext>(
			Type serviceType,
			string? commandName = null,
			Type? handlerType = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TServiceContext : ServiceContext, new()
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var serviceBaseType = typeof(ServiceBase<TServiceContext>);
			if (!serviceBaseType.IsAssignableFrom(serviceType))
				throw new InvalidOperationException($"serviceType {serviceType.FullName} must inherit from {serviceBaseType.FullName}");

			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var appCtx = _serviceFactory.GetRequiredInstance<IApplicationContext>();

			var traceInfo = new TraceInfoBuilder(traceFrame, appCtx.Next()).Build();
			var commandHandlerContextBuilder = CreateCommandHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, commandName, handlerType);
			var commandHandlerContext = commandHandlerContextBuilder.Context;

			var serviceContext = new TServiceContext();
			serviceContext.Init(traceInfo, commandHandlerContext, serviceType);
			return serviceContext;
		}
	}
}
