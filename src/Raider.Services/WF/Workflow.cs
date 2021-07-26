//using Raider.Commands;
//using Raider.Logging;
//using Raider.Logging.Extensions;
//using Raider.Services.Commands;
//using System;
//using System.Runtime.CompilerServices;
//using System.Threading.Tasks;

//namespace Raider.Services
//{
//	public class Workflow
//	{
//		private readonly ServiceContext _serviceContext;
//		private readonly AspectOptions _options;

//		public Workflow(ServiceContext serviceContext)
//			: this(serviceContext, null!)
//		{
//		}

//		public Workflow(ServiceContext serviceContext, Action<AspectOptions> configure)
//		{
//			_serviceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
//			_options = new AspectOptions();
//			configure?.Invoke(_options);
//		}

//		private ICommandResult<OnBeforeAspectContinuation> OnBeforeInvoke(AspectContext ctx)
//			=> _options.OnBeforeInvoke == null
//				? CommandResultBuilder<OnBeforeAspectContinuation>.FromResult(OnBeforeAspectContinuation.CallParentAspect)
//				: _options.OnBeforeInvoke.Invoke(ctx);

//		private Task<ICommandResult<OnBeforeAspectContinuation>> OnBeforeInvokeAsync(AspectContext ctx)
//		{
//			if (_options.OnBeforeInvokeAsync != null)
//				return _options.OnBeforeInvokeAsync(ctx);

//			return Task.FromResult(CommandResultBuilder<OnBeforeAspectContinuation>.FromResult(OnBeforeAspectContinuation.CallParentAspect));
//		}

//		private ICommandResult<OnAfterAspectContinuation> OnAfterInvoke(AspectContext ctx, object? actionResult)
//			=> _options.OnAfterInvoke == null
//				? CommandResultBuilder<OnAfterAspectContinuation>.FromResult(OnAfterAspectContinuation.MergeMessagesAndCallParentAspect)
//				: _options.OnAfterInvoke.Invoke(ctx, actionResult);

//		private Task<ICommandResult<OnAfterAspectContinuation>> OnAfterInvokeAsync(AspectContext ctx, object? actionResult)
//		{
//			if (_options.OnAfterInvokeAsync != null)
//				return _options.OnAfterInvokeAsync(ctx, actionResult);

//			return Task.FromResult(CommandResultBuilder<OnAfterAspectContinuation>.FromResult(OnAfterAspectContinuation.MergeMessagesAndCallParentAspect));
//		}

//		private ExceptionHandlingEnum HandleError(CommandResultBuilder result, MethodLogScope scope, AspectContext ctx, Exception ex)
//		{
//			var exceptionHandling = _options.DefaultExceptionHandling;

//			if (_options.OnError != null)
//			{
//				try
//				{
//					var exceptionHandlingResult = _options.OnError.Invoke(ctx, ex);
//					if (result.MergeHasError(exceptionHandlingResult))
//						return ExceptionHandlingEnum.Throw;

//					if (exceptionHandlingResult.ResultWasSet)
//						exceptionHandling = exceptionHandlingResult.Result;
//				}
//				catch (Exception uex)
//				{
//					result.WithError(scope, x => x.ExceptionInfo(uex));
//					_serviceContext.Logger?.LogErrorMessage(scope, x => x.ExceptionInfo(uex));
//				}
//			}

//			return exceptionHandling;
//		}

//		private ExceptionHandlingEnum HandleError<T>(CommandResultBuilder<T> result, MethodLogScope scope, AspectContext ctx, Exception ex)
//		{
//			var exceptionHandling = _options.DefaultExceptionHandling;

//			if (_options.OnError != null)
//			{
//				try
//				{
//					var exceptionHandlingResult = _options.OnError.Invoke(ctx, ex);
//					if (result.MergeHasError(exceptionHandlingResult))
//						return ExceptionHandlingEnum.Throw;

//					if (exceptionHandlingResult.ResultWasSet)
//						exceptionHandling = exceptionHandlingResult.Result;
//				}
//				catch (Exception uex)
//				{
//					result.WithError(scope, x => x.ExceptionInfo(uex));
//					_serviceContext.Logger?.LogErrorMessage(scope, x => x.ExceptionInfo(uex));
//				}
//			}

//			return exceptionHandling;
//		}

//		public ICommandResult Invoke(
//			Action<AspectContext> action,
//			Action<InterceptorOptions>? configure = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//		{
//			if (action == null)
//				throw new ArgumentNullException(nameof(action));

//			InterceptorOptions? invocationOptions = null;
//			if (configure != null)
//			{
//				invocationOptions = new InterceptorOptions();
//				configure.Invoke(invocationOptions);
//			}

//			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

//			var ctx = new AspectContext(_serviceContext, methodLogScope);
//			var result = new CommandResultBuilder();

//			try
//			{
//				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

//				if (invocationOptions?.OnBeforeInvoke != null)
//				{
//					var onBeforeResult = invocationOptions.OnBeforeInvoke.Invoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();

//					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
//					{
//						onBeforeResult = OnBeforeInvoke(ctx);
//						if (result.MergeHasError(onBeforeResult))
//							return result.Build();

//						if (onBeforeResult.ResultWasSet)
//							onBeforeContinuation = onBeforeResult.Result;

//						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//							return result.Build();
//					}
//				}
//				else
//				{
//					var onBeforeResult = OnBeforeInvoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();
//				}

//				action(ctx);

//				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

//				if (invocationOptions?.OnAfterInvoke != null)
//				{
//					var onAfterResult = invocationOptions.OnAfterInvoke.Invoke(ctx, null);
//					if (result.MergeHasError(onAfterResult))
//						return result.Build();

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//						return result.Build();

//					onAfterResult = OnAfterInvoke(ctx, null);
//					if (result.MergeHasError(onAfterResult))
//						return result.Build();
//				}
//				else
//				{
//					var onAfterResult = OnAfterInvoke(ctx, null);
//					if (result.MergeHasError(onAfterResult))
//						return result.Build();
//				}

//				return result.Build();
//			}
//			catch (Exception ex)
//			{
//				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
//				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

//				var exceptionHandling = invocationOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
//				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

//				if (invocationOptions?.OnError != null)
//				{
//					try
//					{
//						var exceptionHandlingResult = invocationOptions.OnError.Invoke(ctx, ex);
//						result.MergeHasError(exceptionHandlingResult);

//						if (exceptionHandlingResult.ResultWasSet)
//							exceptionHandling = exceptionHandlingResult.Result;
//					}
//					catch (Exception uex)
//					{
//						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
//						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
//					}

//					if (exceptionHandling == ExceptionHandlingEnum.Throw)
//						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}
//				else
//				{
//					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}

//				if (exceptionHandling == ExceptionHandlingEnum.Handle)
//					return result.Build();
//				else
//					throw;
//			}
//		}

//		public ICommandResult<T> Invoke<T>(
//			Func<AspectContext, T> action,
//			Action<InterceptorOptions>? configure = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//		{
//			if (action == null)
//				throw new ArgumentNullException(nameof(action));

//			InterceptorOptions? invocationOptions = null;
//			if (configure != null)
//			{
//				invocationOptions = new InterceptorOptions();
//				configure.Invoke(invocationOptions);
//			}

//			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

//			var ctx = new AspectContext(_serviceContext, methodLogScope);
//			var result = new CommandResultBuilder<T>();

//			try
//			{
//				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

//				if (invocationOptions?.OnBeforeInvoke != null)
//				{
//					var onBeforeResult = invocationOptions.OnBeforeInvoke.Invoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();

//					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
//					{
//						onBeforeResult = OnBeforeInvoke(ctx);
//						if (result.MergeHasError(onBeforeResult))
//							return result.Build();

//						if (onBeforeResult.ResultWasSet)
//							onBeforeContinuation = onBeforeResult.Result;

//						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//							return result.Build();
//					}
//				}
//				else
//				{
//					var onBeforeResult = OnBeforeInvoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();
//				}

//				var actionResult = action(ctx);
//				result.WithResult(actionResult);

//				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

//				if (invocationOptions?.OnAfterInvoke != null)
//				{
//					var onAfterResult = invocationOptions.OnAfterInvoke.Invoke(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//						return result.Build();

//					onAfterResult = OnAfterInvoke(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}
//				}
//				else
//				{
//					var onAfterResult = OnAfterInvoke(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}
//				}

//				return result.Build();
//			}
//			catch (Exception ex)
//			{
//				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
//				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

//				var exceptionHandling = invocationOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
//				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

//				if (invocationOptions?.OnError != null)
//				{
//					try
//					{
//						var exceptionHandlingResult = invocationOptions.OnError.Invoke(ctx, ex);
//						result.MergeHasError(exceptionHandlingResult);

//						if (exceptionHandlingResult.ResultWasSet)
//							exceptionHandling = exceptionHandlingResult.Result;
//					}
//					catch (Exception uex)
//					{
//						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
//						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
//					}

//					if (exceptionHandling == ExceptionHandlingEnum.Throw)
//						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}
//				else
//				{
//					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}

//				if (exceptionHandling == ExceptionHandlingEnum.Handle)
//					return result.Build();
//				else
//					throw;
//			}
//		}

//		public ICommandResult<T> Invoke<T>(
//			Func<AspectContext, ICommandResult<T>> action,
//			Action<InterceptorOptions>? configure = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//		{
//			if (action == null)
//				throw new ArgumentNullException(nameof(action));

//			InterceptorOptions? invocationOptions = null;
//			if (configure != null)
//			{
//				invocationOptions = new InterceptorOptions();
//				configure.Invoke(invocationOptions);
//			}

//			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

//			var ctx = new AspectContext(_serviceContext, methodLogScope);
//			var result = new CommandResultBuilder<T>();

//			try
//			{
//				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

//				if (invocationOptions?.OnBeforeInvoke != null)
//				{
//					var onBeforeResult = invocationOptions.OnBeforeInvoke.Invoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();

//					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
//					{
//						onBeforeResult = OnBeforeInvoke(ctx);
//						if (result.MergeHasError(onBeforeResult))
//							return result.Build();

//						if (onBeforeResult.ResultWasSet)
//							onBeforeContinuation = onBeforeResult.Result;

//						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//							return result.Build();
//					}
//				}
//				else
//				{
//					var onBeforeResult = OnBeforeInvoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();
//				}

//				var actionResult = action(ctx);
//				result.MergeAllHasError(actionResult);

//				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

//				if (invocationOptions?.OnAfterInvoke != null)
//				{
//					var onAfterResult = invocationOptions.OnAfterInvoke.Invoke(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//						return result.Build();

//					onAfterResult = OnAfterInvoke(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}
//				}
//				else
//				{
//					var onAfterResult = OnAfterInvoke(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}
//				}

//				return result.Build();
//			}
//			catch (Exception ex)
//			{
//				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
//				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

//				var exceptionHandling = invocationOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
//				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

//				if (invocationOptions?.OnError != null)
//				{
//					try
//					{
//						var exceptionHandlingResult = invocationOptions.OnError.Invoke(ctx, ex);
//						result.MergeHasError(exceptionHandlingResult);

//						if (exceptionHandlingResult.ResultWasSet)
//							exceptionHandling = exceptionHandlingResult.Result;
//					}
//					catch (Exception uex)
//					{
//						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
//						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
//					}

//					if (exceptionHandling == ExceptionHandlingEnum.Throw)
//						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}
//				else
//				{
//					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}

//				if (exceptionHandling == ExceptionHandlingEnum.Handle)
//					return result.Build();
//				else
//					throw;
//			}
//		}

//		public async Task<ICommandResult> InvokeAsync(
//			Func<AspectContext, Task> action,
//			Action<AsyncInterceptorOptions>? configure = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//		{
//			if (action == null)
//				throw new ArgumentNullException(nameof(action));

//			AsyncInterceptorOptions? invocationOptions = null;
//			if (configure != null)
//			{
//				invocationOptions = new AsyncInterceptorOptions();
//				configure.Invoke(invocationOptions);
//			}

//			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

//			var ctx = new AspectContext(_serviceContext, methodLogScope);
//			var result = new CommandResultBuilder();

//			try
//			{
//				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

//				if (invocationOptions?.OnBeforeInvokeAsync != null)
//				{
//					var onBeforeResult = await invocationOptions.OnBeforeInvokeAsync.Invoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();

//					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
//					{
//						onBeforeResult = await OnBeforeInvokeAsync(ctx);
//						if (result.MergeHasError(onBeforeResult))
//							return result.Build();

//						if (onBeforeResult.ResultWasSet)
//							onBeforeContinuation = onBeforeResult.Result;

//						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//							return result.Build();
//					}
//				}
//				else
//				{
//					var onBeforeResult = await OnBeforeInvokeAsync(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();
//				}

//				await action(ctx);

//				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

//				if (invocationOptions?.OnAfterInvokeAsync != null)
//				{
//					var onAfterResult = await invocationOptions.OnAfterInvokeAsync.Invoke(ctx, null);
//					if (result.MergeHasError(onAfterResult))
//						return result.Build();

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//						return result.Build();

//					onAfterResult = await OnAfterInvokeAsync(ctx, null);
//					if (result.MergeHasError(onAfterResult))
//						return result.Build();
//				}
//				else
//				{
//					var onAfterResult = await OnAfterInvokeAsync(ctx, null);
//					if (result.MergeHasError(onAfterResult))
//						return result.Build();
//				}

//				return result.Build();
//			}
//			catch (Exception ex)
//			{
//				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
//				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

//				var exceptionHandling = invocationOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
//				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

//				if (invocationOptions?.OnError != null)
//				{
//					try
//					{
//						var exceptionHandlingResult = invocationOptions.OnError.Invoke(ctx, ex);
//						result.MergeHasError(exceptionHandlingResult);

//						if (exceptionHandlingResult.ResultWasSet)
//							exceptionHandling = exceptionHandlingResult.Result;
//					}
//					catch (Exception uex)
//					{
//						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
//						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
//					}

//					if (exceptionHandling == ExceptionHandlingEnum.Throw)
//						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}
//				else
//				{
//					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}

//				if (exceptionHandling == ExceptionHandlingEnum.Handle)
//					return result.Build();
//				else
//					throw;
//			}
//		}

//		public async Task<ICommandResult<T>> InvokeAsync<T>(
//			Func<AspectContext, Task<T>> action,
//			Action<AsyncInterceptorOptions>? configure = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//		{
//			if (action == null)
//				throw new ArgumentNullException(nameof(action));

//			AsyncInterceptorOptions? invocationOptions = null;
//			if (configure != null)
//			{
//				invocationOptions = new AsyncInterceptorOptions();
//				configure.Invoke(invocationOptions);
//			}

//			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

//			var ctx = new AspectContext(_serviceContext, methodLogScope);
//			var result = new CommandResultBuilder<T>();

//			try
//			{
//				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

//				if (invocationOptions?.OnBeforeInvokeAsync != null)
//				{
//					var onBeforeResult = await invocationOptions.OnBeforeInvokeAsync.Invoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();

//					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
//					{
//						onBeforeResult = await OnBeforeInvokeAsync(ctx);
//						if (result.MergeHasError(onBeforeResult))
//							return result.Build();

//						if (onBeforeResult.ResultWasSet)
//							onBeforeContinuation = onBeforeResult.Result;

//						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//							return result.Build();
//					}
//				}
//				else
//				{
//					var onBeforeResult = await OnBeforeInvokeAsync(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();
//				}

//				var actionResult = await action(ctx);
//				result.WithResult(actionResult);

//				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

//				if (invocationOptions?.OnAfterInvokeAsync != null)
//				{
//					var onAfterResult = await invocationOptions.OnAfterInvokeAsync.Invoke(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//						return result.Build();

//					onAfterResult = await OnAfterInvokeAsync(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}
//				}
//				else
//				{
//					var onAfterResult = await OnAfterInvokeAsync(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}
//				}

//				return result.Build();
//			}
//			catch (Exception ex)
//			{
//				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
//				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

//				var exceptionHandling = invocationOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
//				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

//				if (invocationOptions?.OnError != null)
//				{
//					try
//					{
//						var exceptionHandlingResult = invocationOptions.OnError.Invoke(ctx, ex);
//						result.MergeHasError(exceptionHandlingResult);

//						if (exceptionHandlingResult.ResultWasSet)
//							exceptionHandling = exceptionHandlingResult.Result;
//					}
//					catch (Exception uex)
//					{
//						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
//						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
//					}

//					if (exceptionHandling == ExceptionHandlingEnum.Throw)
//						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}
//				else
//				{
//					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}

//				if (exceptionHandling == ExceptionHandlingEnum.Handle)
//					return result.Build();
//				else
//					throw;
//			}
//		}

//		public async Task<ICommandResult<T>> InvokeAsync<T>(
//			Func<AspectContext, Task<ICommandResult<T>>> action,
//			Action<AsyncInterceptorOptions>? configure = null,
//			[CallerMemberName] string memberName = "",
//			[CallerFilePath] string sourceFilePath = "",
//			[CallerLineNumber] int sourceLineNumber = 0)
//		{
//			if (action == null)
//				throw new ArgumentNullException(nameof(action));

//			AsyncInterceptorOptions? invocationOptions = null;
//			if (configure != null)
//			{
//				invocationOptions = new AsyncInterceptorOptions();
//				configure.Invoke(invocationOptions);
//			}

//			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

//			var ctx = new AspectContext(_serviceContext, methodLogScope);
//			var result = new CommandResultBuilder<T>();

//			try
//			{
//				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

//				if (invocationOptions?.OnBeforeInvokeAsync != null)
//				{
//					var onBeforeResult = await invocationOptions .OnBeforeInvokeAsync.Invoke(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();

//					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
//					{
//						onBeforeResult = await OnBeforeInvokeAsync(ctx);
//						if (result.MergeHasError(onBeforeResult))
//							return result.Build();

//						if (onBeforeResult.ResultWasSet)
//							onBeforeContinuation = onBeforeResult.Result;

//						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//							return result.Build();
//					}
//				}
//				else
//				{
//					var onBeforeResult = await OnBeforeInvokeAsync(ctx);
//					if (result.MergeHasError(onBeforeResult))
//						return result.Build();

//					if (onBeforeResult.ResultWasSet)
//						onBeforeContinuation = onBeforeResult.Result;

//					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
//						return result.Build();
//				}

//				var actionResult = await action(ctx);
//				result.MergeAllHasError(actionResult);

//				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

//				if (invocationOptions?.OnAfterInvokeAsync != null)
//				{
//					var onAfterResult = await invocationOptions.OnAfterInvokeAsync.Invoke(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//						return result.Build();

//					onAfterResult = await OnAfterInvokeAsync(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}
//				}
//				else
//				{
//					var onAfterResult = await OnAfterInvokeAsync(ctx, actionResult);
//					result.MergeHasError(onAfterResult);

//					if (onAfterResult.ResultWasSet)
//						onAfterContinuation = onAfterResult.Result;

//					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
//						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
//					{
//						result.ClearResult();
//					}
//				}

//				return result.Build();
//			}
//			catch (Exception ex)
//			{
//				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
//				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

//				var exceptionHandling = invocationOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
//				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

//				if (invocationOptions?.OnError != null)
//				{
//					try
//					{
//						var exceptionHandlingResult = invocationOptions.OnError.Invoke(ctx, ex);
//						result.MergeHasError(exceptionHandlingResult);

//						if (exceptionHandlingResult.ResultWasSet)
//							exceptionHandling = exceptionHandlingResult.Result;
//					}
//					catch (Exception uex)
//					{
//						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
//						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
//					}

//					if (exceptionHandling == ExceptionHandlingEnum.Throw)
//						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}
//				else
//				{
//					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
//				}

//				if (exceptionHandling == ExceptionHandlingEnum.Handle)
//					return result.Build();
//				else
//					throw;
//			}
//		}
//	}
//}
