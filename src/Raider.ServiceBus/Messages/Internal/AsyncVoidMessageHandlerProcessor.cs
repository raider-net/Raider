using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.Messages.Interceptors;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages.Internal
{
	internal abstract class AsyncVoidMessageHandlerProcessor : MessageHandlerProcessorBase
	{
		public abstract Task<IResult> HandleAsync(
			Messages.IRequestMessage message,
			IMessageHandlerContext handlerContext,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken = default);
	}

	internal class AsyncVoidMessageHandlerProcessor<TRequestMessage, TContext> : AsyncVoidMessageHandlerProcessor
		where TRequestMessage : Messages.IRequestMessage
		where TContext : IMessageHandlerContext
	{
		protected override IMessageHandler CreateHandler(IServiceProvider serviceProvider)
		{
			var handler = serviceProvider.GetService<IAsyncMessageHandler<TRequestMessage, TContext>>();
			if (handler == null)
				throw new InvalidOperationException($"Could not resolve handler for {typeof(IAsyncMessageHandler<TRequestMessage, TContext>).FullName}");

			return handler;
		}

		public override Task<IResult> HandleAsync(
			Messages.IRequestMessage message,
			IMessageHandlerContext handlerContext,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken = default)
			=> HandleAsync((TRequestMessage)message, (TContext)handlerContext, serviceProvider, cancellationToken);

		public async Task<IResult> HandleAsync(
			TRequestMessage message,
			TContext handlerContext,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var handler = (IAsyncMessageHandler<TRequestMessage, TContext>)CreateHandler(serviceProvider);

				IResult result;
				var interceptorType = handler.InterceptorType;
				if (interceptorType == null)
				{
					result = await handler.HandleAsync(message, handlerContext, cancellationToken);
				}
				else
				{
					var interceptor = (IAsyncMessageHandlerInterceptor<TRequestMessage, TContext>?)serviceProvider.GetService(interceptorType);
					if (interceptor == null)
						throw new InvalidOperationException($"Could not resolve interceptor for {typeof(IAsyncMessageHandlerInterceptor<TRequestMessage, TContext>).FullName}");

					result = await interceptor.InterceptHandleAsync(message, handlerContext, handler.HandleAsync, cancellationToken);
				}

				return result;
			}
			catch (Exception exHandler)
			{
				await handlerContext.LogErrorAsync(TraceInfo.Create(handlerContext.PreviousTraceInfo), MessageStatus.Aborted, x => x.ExceptionInfo(exHandler), "SendAsync<Messages.IRequestMessage> error", null, cancellationToken);
				throw;
			}
		}
	}
}
