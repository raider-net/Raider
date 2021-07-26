using Raider.Commands;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Services.Commands;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Raider.Services.Aspects
{
	public class InterceptorBase : AspectBase
	{
		public InterceptorBase(ServiceContext serviceContext)
			: base(serviceContext, null!)
		{
		}

		public InterceptorBase(ServiceContext serviceContext, Action<AspectOptions> configure)
			: base(serviceContext, configure)
		{
		}

		public override ICommandResult Invoke(
			Action<AspectContext> action,
			Action<InterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			InterceptorOptions? interceptorOptions = null;
			if (configure != null)
			{
				interceptorOptions = new InterceptorOptions();
				configure.Invoke(interceptorOptions);
			}

			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

			var ctx = new AspectContext(_serviceContext, methodLogScope);
			var result = new CommandResultBuilder();

			try
			{
				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

				if (interceptorOptions?.OnBeforeInvoke != null)
				{
					var onBeforeResult = interceptorOptions.OnBeforeInvoke.Invoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();

					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
					{
						onBeforeResult = OnBeforeInvoke(ctx);
						if (result.MergeHasError(onBeforeResult))
							return result.Build();

						if (onBeforeResult.ResultWasSet)
							onBeforeContinuation = onBeforeResult.Result;

						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
							return result.Build();
					}
				}
				else
				{
					var onBeforeResult = OnBeforeInvoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();
				}

				action(ctx);

				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

				if (interceptorOptions?.OnAfterInvoke != null)
				{
					var onAfterResult = interceptorOptions.OnAfterInvoke.Invoke(ctx, null);
					if (result.MergeHasError(onAfterResult))
						return result.Build();

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
						return result.Build();

					onAfterResult = OnAfterInvoke(ctx, null);
					if (result.MergeHasError(onAfterResult))
						return result.Build();
				}
				else
				{
					var onAfterResult = OnAfterInvoke(ctx, null);
					if (result.MergeHasError(onAfterResult))
						return result.Build();
				}

				return result.Build();
			}
			catch (Exception ex)
			{
				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

				var exceptionHandling = interceptorOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

				if (interceptorOptions?.OnError != null)
				{
					try
					{
						var exceptionHandlingResult = interceptorOptions.OnError.Invoke(ctx, ex);
						result.MergeHasError(exceptionHandlingResult);

						if (exceptionHandlingResult.ResultWasSet)
							exceptionHandling = exceptionHandlingResult.Result;
					}
					catch (Exception uex)
					{
						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
					}

					if (exceptionHandling == ExceptionHandlingEnum.Throw)
						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}
				else
				{
					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}

				if (exceptionHandling == ExceptionHandlingEnum.Handle)
					return result.Build();
				else
					throw;
			}
		}

		public override ICommandResult<T> Invoke<T>(
			Func<AspectContext, T> action,
			Action<InterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			InterceptorOptions? interceptorOptions = null;
			if (configure != null)
			{
				interceptorOptions = new InterceptorOptions();
				configure.Invoke(interceptorOptions);
			}

			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

			var ctx = new AspectContext(_serviceContext, methodLogScope);
			var result = new CommandResultBuilder<T>();

			try
			{
				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

				if (interceptorOptions?.OnBeforeInvoke != null)
				{
					var onBeforeResult = interceptorOptions.OnBeforeInvoke.Invoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();

					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
					{
						onBeforeResult = OnBeforeInvoke(ctx);
						if (result.MergeHasError(onBeforeResult))
							return result.Build();

						if (onBeforeResult.ResultWasSet)
							onBeforeContinuation = onBeforeResult.Result;

						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
							return result.Build();
					}
				}
				else
				{
					var onBeforeResult = OnBeforeInvoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();
				}

				var actionResult = action(ctx);
				result.WithResult(actionResult);

				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

				if (interceptorOptions?.OnAfterInvoke != null)
				{
					var onAfterResult = interceptorOptions.OnAfterInvoke.Invoke(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
						return result.Build();

					onAfterResult = OnAfterInvoke(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}
				}
				else
				{
					var onAfterResult = OnAfterInvoke(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}
				}

				return result.Build();
			}
			catch (Exception ex)
			{
				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

				var exceptionHandling = interceptorOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

				if (interceptorOptions?.OnError != null)
				{
					try
					{
						var exceptionHandlingResult = interceptorOptions.OnError.Invoke(ctx, ex);
						result.MergeHasError(exceptionHandlingResult);

						if (exceptionHandlingResult.ResultWasSet)
							exceptionHandling = exceptionHandlingResult.Result;
					}
					catch (Exception uex)
					{
						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
					}

					if (exceptionHandling == ExceptionHandlingEnum.Throw)
						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}
				else
				{
					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}

				if (exceptionHandling == ExceptionHandlingEnum.Handle)
					return result.Build();
				else
					throw;
			}
		}

		public override ICommandResult<T> Invoke<T>(
			Func<AspectContext, ICommandResult<T>> action,
			Action<InterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			InterceptorOptions? interceptorOptions = null;
			if (configure != null)
			{
				interceptorOptions = new InterceptorOptions();
				configure.Invoke(interceptorOptions);
			}

			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

			var ctx = new AspectContext(_serviceContext, methodLogScope);
			var result = new CommandResultBuilder<T>();

			try
			{
				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

				if (interceptorOptions?.OnBeforeInvoke != null)
				{
					var onBeforeResult = interceptorOptions.OnBeforeInvoke.Invoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();

					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
					{
						onBeforeResult = OnBeforeInvoke(ctx);
						if (result.MergeHasError(onBeforeResult))
							return result.Build();

						if (onBeforeResult.ResultWasSet)
							onBeforeContinuation = onBeforeResult.Result;

						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
							return result.Build();
					}
				}
				else
				{
					var onBeforeResult = OnBeforeInvoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();
				}

				var actionResult = action(ctx);
				result.MergeAllHasError(actionResult);

				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

				if (interceptorOptions?.OnAfterInvoke != null)
				{
					var onAfterResult = interceptorOptions.OnAfterInvoke.Invoke(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
						return result.Build();

					onAfterResult = OnAfterInvoke(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}
				}
				else
				{
					var onAfterResult = OnAfterInvoke(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}
				}

				return result.Build();
			}
			catch (Exception ex)
			{
				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

				var exceptionHandling = interceptorOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

				if (interceptorOptions?.OnError != null)
				{
					try
					{
						var exceptionHandlingResult = interceptorOptions.OnError.Invoke(ctx, ex);
						result.MergeHasError(exceptionHandlingResult);

						if (exceptionHandlingResult.ResultWasSet)
							exceptionHandling = exceptionHandlingResult.Result;
					}
					catch (Exception uex)
					{
						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
					}

					if (exceptionHandling == ExceptionHandlingEnum.Throw)
						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}
				else
				{
					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}

				if (exceptionHandling == ExceptionHandlingEnum.Handle)
					return result.Build();
				else
					throw;
			}
		}

		public override async Task<ICommandResult> InvokeAsync(
			Func<AspectContext, Task> action,
			Action<AsyncInterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			AsyncInterceptorOptions? interceptorOptions = null;
			if (configure != null)
			{
				interceptorOptions = new AsyncInterceptorOptions();
				configure.Invoke(interceptorOptions);
			}

			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

			var ctx = new AspectContext(_serviceContext, methodLogScope);
			var result = new CommandResultBuilder();

			try
			{
				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

				if (interceptorOptions?.OnBeforeInvokeAsync != null)
				{
					var onBeforeResult = await interceptorOptions.OnBeforeInvokeAsync.Invoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();

					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
					{
						onBeforeResult = await OnBeforeInvokeAsync(ctx);
						if (result.MergeHasError(onBeforeResult))
							return result.Build();

						if (onBeforeResult.ResultWasSet)
							onBeforeContinuation = onBeforeResult.Result;

						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
							return result.Build();
					}
				}
				else
				{
					var onBeforeResult = await OnBeforeInvokeAsync(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();
				}

				await action(ctx);

				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

				if (interceptorOptions?.OnAfterInvokeAsync != null)
				{
					var onAfterResult = await interceptorOptions.OnAfterInvokeAsync.Invoke(ctx, null);
					if (result.MergeHasError(onAfterResult))
						return result.Build();

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
						return result.Build();

					onAfterResult = await OnAfterInvokeAsync(ctx, null);
					if (result.MergeHasError(onAfterResult))
						return result.Build();
				}
				else
				{
					var onAfterResult = await OnAfterInvokeAsync(ctx, null);
					if (result.MergeHasError(onAfterResult))
						return result.Build();
				}

				return result.Build();
			}
			catch (Exception ex)
			{
				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

				var exceptionHandling = interceptorOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

				if (interceptorOptions?.OnError != null)
				{
					try
					{
						var exceptionHandlingResult = interceptorOptions.OnError.Invoke(ctx, ex);
						result.MergeHasError(exceptionHandlingResult);

						if (exceptionHandlingResult.ResultWasSet)
							exceptionHandling = exceptionHandlingResult.Result;
					}
					catch (Exception uex)
					{
						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
					}

					if (exceptionHandling == ExceptionHandlingEnum.Throw)
						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}
				else
				{
					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}

				if (exceptionHandling == ExceptionHandlingEnum.Handle)
					return result.Build();
				else
					throw;
			}
		}

		public override async Task<ICommandResult<T>> InvokeAsync<T>(
			Func<AspectContext, Task<T>> action,
			Action<AsyncInterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			AsyncInterceptorOptions? interceptorOptions = null;
			if (configure != null)
			{
				interceptorOptions = new AsyncInterceptorOptions();
				configure.Invoke(interceptorOptions);
			}

			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

			var ctx = new AspectContext(_serviceContext, methodLogScope);
			var result = new CommandResultBuilder<T>();

			try
			{
				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

				if (interceptorOptions?.OnBeforeInvokeAsync != null)
				{
					var onBeforeResult = await interceptorOptions.OnBeforeInvokeAsync.Invoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();

					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
					{
						onBeforeResult = await OnBeforeInvokeAsync(ctx);
						if (result.MergeHasError(onBeforeResult))
							return result.Build();

						if (onBeforeResult.ResultWasSet)
							onBeforeContinuation = onBeforeResult.Result;

						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
							return result.Build();
					}
				}
				else
				{
					var onBeforeResult = await OnBeforeInvokeAsync(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();
				}

				var actionResult = await action(ctx);
				result.WithResult(actionResult);

				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

				if (interceptorOptions?.OnAfterInvokeAsync != null)
				{
					var onAfterResult = await interceptorOptions.OnAfterInvokeAsync.Invoke(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
						return result.Build();

					onAfterResult = await OnAfterInvokeAsync(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}
				}
				else
				{
					var onAfterResult = await OnAfterInvokeAsync(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}
				}

				return result.Build();
			}
			catch (Exception ex)
			{
				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

				var exceptionHandling = interceptorOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

				if (interceptorOptions?.OnError != null)
				{
					try
					{
						var exceptionHandlingResult = interceptorOptions.OnError.Invoke(ctx, ex);
						result.MergeHasError(exceptionHandlingResult);

						if (exceptionHandlingResult.ResultWasSet)
							exceptionHandling = exceptionHandlingResult.Result;
					}
					catch (Exception uex)
					{
						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
					}

					if (exceptionHandling == ExceptionHandlingEnum.Throw)
						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}
				else
				{
					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}

				if (exceptionHandling == ExceptionHandlingEnum.Handle)
					return result.Build();
				else
					throw;
			}
		}

		public override async Task<ICommandResult<T>> InvokeAsync<T>(
			Func<AspectContext, Task<ICommandResult<T>>> action,
			Action<AsyncInterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			AsyncInterceptorOptions? interceptorOptions = null;
			if (configure != null)
			{
				interceptorOptions = new AsyncInterceptorOptions();
				configure.Invoke(interceptorOptions);
			}

			using var methodLogScope = _serviceContext.CreateScope((MethodLogScope?)null, null, memberName, sourceFilePath, sourceLineNumber);

			var ctx = new AspectContext(_serviceContext, methodLogScope);
			var result = new CommandResultBuilder<T>();

			try
			{
				var onBeforeContinuation = OnBeforeAspectContinuation.CallParentAspect;

				if (interceptorOptions?.OnBeforeInvokeAsync != null)
				{
					var onBeforeResult = await interceptorOptions.OnBeforeInvokeAsync.Invoke(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();

					if (onBeforeContinuation == OnBeforeAspectContinuation.CallParentAspect)
					{
						onBeforeResult = await OnBeforeInvokeAsync(ctx);
						if (result.MergeHasError(onBeforeResult))
							return result.Build();

						if (onBeforeResult.ResultWasSet)
							onBeforeContinuation = onBeforeResult.Result;

						if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
							return result.Build();
					}
				}
				else
				{
					var onBeforeResult = await OnBeforeInvokeAsync(ctx);
					if (result.MergeHasError(onBeforeResult))
						return result.Build();

					if (onBeforeResult.ResultWasSet)
						onBeforeContinuation = onBeforeResult.Result;

					if (onBeforeContinuation == OnBeforeAspectContinuation.StopExecution)
						return result.Build();
				}

				var actionResult = await action(ctx);
				result.MergeAllHasError(actionResult);

				var onAfterContinuation = OnAfterAspectContinuation.MergeMessagesAndCallParentAspect;

				if (interceptorOptions?.OnAfterInvokeAsync != null)
				{
					var onAfterResult = await interceptorOptions.OnAfterInvokeAsync.Invoke(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesAndSkipParentAspects
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
						return result.Build();

					onAfterResult = await OnAfterInvokeAsync(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}
				}
				else
				{
					var onAfterResult = await OnAfterInvokeAsync(ctx, actionResult);
					result.MergeHasError(onAfterResult);

					if (onAfterResult.ResultWasSet)
						onAfterContinuation = onAfterResult.Result;

					if (onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndCallParentAspect
						|| onAfterContinuation == OnAfterAspectContinuation.MergeMessagesRemoveResultAndSkipParentAspects)
					{
						result.ClearResult();
					}
				}

				return result.Build();
			}
			catch (Exception ex)
			{
				using var exceptionLogScope = _serviceContext.CreateScope(methodLogScope); //vytovorim scope pre unhandled exceptiony
				result.WithError(exceptionLogScope, x => x.ExceptionInfo(ex));

				var exceptionHandling = interceptorOptions?.DefaultExceptionHandling ?? _options.DefaultExceptionHandling;
				_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(ex));

				if (interceptorOptions?.OnError != null)
				{
					try
					{
						var exceptionHandlingResult = interceptorOptions.OnError.Invoke(ctx, ex);
						result.MergeHasError(exceptionHandlingResult);

						if (exceptionHandlingResult.ResultWasSet)
							exceptionHandling = exceptionHandlingResult.Result;
					}
					catch (Exception uex)
					{
						result.WithError(exceptionLogScope, x => x.ExceptionInfo(uex));
						_serviceContext.Logger?.LogErrorMessage(exceptionLogScope, x => x.ExceptionInfo(uex));
					}

					if (exceptionHandling == ExceptionHandlingEnum.Throw)
						exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}
				else
				{
					exceptionHandling = HandleError(result, exceptionLogScope, ctx, ex);
				}

				if (exceptionHandling == ExceptionHandlingEnum.Handle)
					return result.Build();
				else
					throw;
			}
		}
	}
}
