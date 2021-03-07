using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.Localization;
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

			commandHandlerContextBuilder
				.TraceInfo(traceInfo)
				.Principal(_applicationContext.Principal)
				.User(_applicationContext.User)
				.Logger(_loggerFactory.CreateLogger(handlerType ?? typeof(TContext)))
				.ApplicationResources(_applicationResources)
				.CommandName(commandName);

			return commandHandlerContextBuilder;
		}

		public ServiceContext CreateServiceContext<TService, THandlerContext, TBuilder>(
			string? commandName = null,
			Type? handlerType = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TService : ServiceBase
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
		{
			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var tc = _serviceFactory.GetInstance<TraceContext>();

			var traceInfo = new TraceInfoBuilder(traceFrame, tc?.Next()).Build();
			var commandHandlerContextBuilder = CreateCommandHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, commandName, handlerType);
			var commandHandlerContext = commandHandlerContextBuilder.Context;

			var serviceContext = new ServiceContext(traceInfo, commandHandlerContext, typeof(TService));
			return serviceContext;
		}

		public ServiceContext CreateServiceContext<THandlerContext, TBuilder>(
			Type serviceType,
			string? commandName = null,
			Type? handlerType = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var serviceBaseType = typeof(ServiceBase);
			if (!serviceBaseType.IsAssignableFrom(serviceType))
				throw new InvalidOperationException($"serviceType {serviceType.FullName} must inherit from {serviceBaseType.FullName}");

			var traceFrame = new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

			var tc = _serviceFactory.GetInstance<TraceContext>();

			var traceInfo = new TraceInfoBuilder(traceFrame, tc?.Next()).Build();
			var commandHandlerContextBuilder = CreateCommandHandlerContextBuilder<THandlerContext, TBuilder>(traceInfo, commandName, handlerType);
			var commandHandlerContext = commandHandlerContextBuilder.Context;

			var serviceContext = new ServiceContext(traceInfo, commandHandlerContext, serviceType);
			return serviceContext;
		}
	}
}
