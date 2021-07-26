using Raider.Commands;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Services.Commands;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Raider.Services.Aspects
{
	public abstract class AspectBase
	{
		protected readonly ServiceContext _serviceContext;
		protected readonly AspectOptions _options;

		protected AspectBase(ServiceContext serviceContext)
			: this(serviceContext, null!)
		{
		}

		protected AspectBase(ServiceContext serviceContext, Action<AspectOptions> configure)
		{
			_serviceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
			_options = new AspectOptions();
			configure?.Invoke(_options);
		}

		protected ICommandResult<OnBeforeAspectContinuation> OnBeforeInvoke(AspectContext ctx)
			=> _options.OnBeforeInvoke == null
				? CommandResultBuilder<OnBeforeAspectContinuation>.FromResult(OnBeforeAspectContinuation.CallParentAspect)
				: _options.OnBeforeInvoke.Invoke(ctx);

		protected Task<ICommandResult<OnBeforeAspectContinuation>> OnBeforeInvokeAsync(AspectContext ctx)
		{
			if (_options.OnBeforeInvokeAsync != null)
				return _options.OnBeforeInvokeAsync(ctx);

			return Task.FromResult(CommandResultBuilder<OnBeforeAspectContinuation>.FromResult(OnBeforeAspectContinuation.CallParentAspect));
		}

		protected ICommandResult<OnAfterAspectContinuation> OnAfterInvoke(AspectContext ctx, object? actionResult)
			=> _options.OnAfterInvoke == null
				? CommandResultBuilder<OnAfterAspectContinuation>.FromResult(OnAfterAspectContinuation.MergeMessagesAndCallParentAspect)
				: _options.OnAfterInvoke.Invoke(ctx, actionResult);

		protected Task<ICommandResult<OnAfterAspectContinuation>> OnAfterInvokeAsync(AspectContext ctx, object? actionResult)
		{
			if (_options.OnAfterInvokeAsync != null)
				return _options.OnAfterInvokeAsync(ctx, actionResult);

			return Task.FromResult(CommandResultBuilder<OnAfterAspectContinuation>.FromResult(OnAfterAspectContinuation.MergeMessagesAndCallParentAspect));
		}

		protected ExceptionHandlingEnum HandleError(CommandResultBuilder result, MethodLogScope scope, AspectContext ctx, Exception ex)
		{
			var exceptionHandling = _options.DefaultExceptionHandling;

			if (_options.OnError != null)
			{
				try
				{
					var exceptionHandlingResult = _options.OnError.Invoke(ctx, ex);
					if (result.MergeHasError(exceptionHandlingResult))
						return ExceptionHandlingEnum.Throw;

					if (exceptionHandlingResult.ResultWasSet)
						exceptionHandling = exceptionHandlingResult.Result;
				}
				catch (Exception uex)
				{
					result.WithError(scope, x => x.ExceptionInfo(uex));
					_serviceContext.Logger?.LogErrorMessage(scope, x => x.ExceptionInfo(uex));
				}
			}

			return exceptionHandling;
		}

		protected ExceptionHandlingEnum HandleError<T>(CommandResultBuilder<T> result, MethodLogScope scope, AspectContext ctx, Exception ex)
		{
			var exceptionHandling = _options.DefaultExceptionHandling;

			if (_options.OnError != null)
			{
				try
				{
					var exceptionHandlingResult = _options.OnError.Invoke(ctx, ex);
					if (result.MergeHasError(exceptionHandlingResult))
						return ExceptionHandlingEnum.Throw;

					if (exceptionHandlingResult.ResultWasSet)
						exceptionHandling = exceptionHandlingResult.Result;
				}
				catch (Exception uex)
				{
					result.WithError(scope, x => x.ExceptionInfo(uex));
					_serviceContext.Logger?.LogErrorMessage(scope, x => x.ExceptionInfo(uex));
				}
			}

			return exceptionHandling;
		}

		public abstract ICommandResult Invoke(
			Action<AspectContext> action,
			Action<InterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		public abstract ICommandResult<T> Invoke<T>(
			Func<AspectContext, T> action,
			Action<InterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		public abstract ICommandResult<T> Invoke<T>(
			Func<AspectContext, ICommandResult<T>> action,
			Action<InterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		public abstract Task<ICommandResult> InvokeAsync(
			Func<AspectContext, Task> action,
			Action<AsyncInterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		public abstract Task<ICommandResult<T>> InvokeAsync<T>(
			Func<AspectContext, Task<T>> action,
			Action<AsyncInterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		public abstract Task<ICommandResult<T>> InvokeAsync<T>(
			Func<AspectContext, Task<ICommandResult<T>>> action,
			Action<AsyncInterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);
	}
}
