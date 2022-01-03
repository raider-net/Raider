using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.Messages.Interceptors;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages.Internal
{
	internal abstract class AsyncMessageHandlerProcessor<TResponse> : MessageHandlerProcessorBase
	{
		public abstract Task<IResult<TResponse>> HandleAsync(
			Messages.IRequestMessage<TResponse> message,
			IMessageHandlerContext handlerContext,
			IServiceProvider serviceProvider,
			Func<IResult<TResponse>, IMessageHandlerContext, CancellationToken, Task<Guid>> saveResponseMessageAction,
			CancellationToken cancellationToken = default);
	}

	internal class AsyncMessageHandlerProcessor<TRequestMessage, TResponse, TContext> : AsyncMessageHandlerProcessor<TResponse>
		where TRequestMessage : Messages.IRequestMessage<TResponse>
		where TContext : IMessageHandlerContext
	{
		protected override IMessageHandler CreateHandler(IServiceProvider serviceProvider)
		{
			var handler = serviceProvider.GetService<IAsyncMessageHandler<TRequestMessage, TResponse, TContext>>();
			if (handler == null)
				throw new InvalidOperationException($"Could not resolve handler for {typeof(IAsyncMessageHandler<TRequestMessage, TResponse, TContext>).FullName}");

			return handler;
		}

		public override Task<IResult<TResponse>> HandleAsync(
			Messages.IRequestMessage<TResponse> message,
			IMessageHandlerContext handlerContext,
			IServiceProvider serviceProvider,
			Func<IResult<TResponse>, IMessageHandlerContext, CancellationToken, Task<Guid>> saveResponseMessageAction,
			CancellationToken cancellationToken = default)
			=> HandleAsync((TRequestMessage)message, (TContext)handlerContext, serviceProvider, saveResponseMessageAction, cancellationToken);

		public async Task<IResult<TResponse>> HandleAsync(
			TRequestMessage message,
			TContext handlerContext,
			IServiceProvider serviceProvider,
			Func<IResult<TResponse>, IMessageHandlerContext, CancellationToken, Task<Guid>> saveResponseMessageAction,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var handler = (IAsyncMessageHandler<TRequestMessage, TResponse, TContext>)CreateHandler(serviceProvider);

				IResult<TResponse> result;
				var interceptorType = handler.InterceptorType;
				if (interceptorType == null)
				{
					result = await handler.HandleAsync(message, handlerContext, cancellationToken);
				}
				else
				{
					var interceptor = (IAsyncMessageHandlerInterceptor<TRequestMessage, TResponse, TContext>?)serviceProvider.GetService(interceptorType);
					if (interceptor == null)
						throw new InvalidOperationException($"Could not resolve interceptor for {typeof(IAsyncMessageHandlerInterceptor<TRequestMessage, TResponse, TContext>).FullName}");

					result = await interceptor.InterceptHandleAsync(message, handlerContext, handler.HandleAsync, cancellationToken);
				}

				await saveResponseMessageAction(result, handlerContext, cancellationToken);

				return result;
			}
			catch (Exception exHandler)
			{
				await handlerContext.LogErrorAsync(TraceInfo.Create(handlerContext.PreviousTraceInfo), MessageStatus.Aborted, x => x.ExceptionInfo(exHandler), "SendAsync<Messages.IRequestMessage<TResponse>> error", null, cancellationToken);
				throw;
			}
		}
	}
}
