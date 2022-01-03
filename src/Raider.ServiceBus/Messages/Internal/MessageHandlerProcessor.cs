using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.Messages.Interceptors;
using Raider.Trace;
using System;

namespace Raider.ServiceBus.Messages.Internal
{
	internal abstract class MessageHandlerProcessor<TResponse> : MessageHandlerProcessorBase
	{
		public abstract IResult<TResponse> Handle(
			Messages.IRequestMessage<TResponse> message,
			IMessageHandlerContext handlerContext,
			IServiceProvider serviceProvider,
			Func<IResult<TResponse>, IMessageHandlerContext, Guid> saveResponseMessageAction);
	}

	internal class MessageHandlerProcessor<TRequestMessage, TResponse, TContext> : MessageHandlerProcessor<TResponse>
		where TRequestMessage : Messages.IRequestMessage<TResponse>
		where TContext : IMessageHandlerContext
	{
		protected override IMessageHandler CreateHandler(IServiceProvider serviceProvider)
		{
			var handler = serviceProvider.GetService<IMessageHandler<TRequestMessage, TResponse, TContext>>();
			if (handler == null)
				throw new InvalidOperationException($"Could not resolve handler for {typeof(IMessageHandler<TRequestMessage, TResponse, TContext>).FullName}");

			return handler;
		}

		public override IResult<TResponse> Handle(
			Messages.IRequestMessage<TResponse> message,
			IMessageHandlerContext handlerContext,
			IServiceProvider serviceProvider,
			Func<IResult<TResponse>, IMessageHandlerContext, Guid> saveResponseMessageAction)
			=> Handle((TRequestMessage)message, (TContext)handlerContext, serviceProvider, saveResponseMessageAction);

		public IResult<TResponse> Handle(
			TRequestMessage message,
			TContext handlerContext,
			IServiceProvider serviceProvider,
			Func<IResult<TResponse>, IMessageHandlerContext, Guid> saveResponseMessageAction)
		{
			try
			{
				var handler = (IMessageHandler<TRequestMessage, TResponse, TContext>)CreateHandler(serviceProvider);

				IResult<TResponse> result;
				var interceptorType = handler.InterceptorType;
				if (interceptorType == null)
				{
					result = handler.Handle(message, handlerContext);
				}
				else
				{
					var interceptor = (IMessageHandlerInterceptor<TRequestMessage, TResponse, TContext>?)serviceProvider.GetService(interceptorType);
					if (interceptor == null)
						throw new InvalidOperationException($"Could not resolve interceptor for {typeof(IMessageHandlerInterceptor<TRequestMessage, TResponse, TContext>).FullName}");

					result = interceptor.InterceptHandle(message, handlerContext, handler.Handle);
				}

				saveResponseMessageAction(result, handlerContext);

				return result;
			}
			catch (Exception exHandler)
			{
				handlerContext.LogError(TraceInfo.Create(handlerContext.PreviousTraceInfo), MessageStatus.Aborted, x => x.ExceptionInfo(exHandler), "Send<Messages.IRequestMessage<TResponse>> error", null);
				throw;
			}
		}
	}
}
