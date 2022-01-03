using Microsoft.Extensions.DependencyInjection;
using Raider.ServiceBus.Events.Interceptors;
using Raider.ServiceBus.Messages;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.ServiceBus.Events.Internal
{
	internal abstract class EventHandlerProcessor : EventHandlerProcessorBase
	{
		public abstract IResult Handle(
			IEvent @vent,
			IEventHandlerContext handlerContext,
			IServiceProvider serviceProvider);
	}

	internal class EventHandlerProcessor<TEvent, TContext> : EventHandlerProcessor
		where TEvent : IEvent
		where TContext : IEventHandlerContext
	{
		protected override IEnumerable<IEventHandler> CreateHandlers(IServiceProvider serviceProvider)
		{
			var handlers = serviceProvider.GetServices<IEventHandler<TEvent, TContext>>();
			if (handlers == null)
				throw new InvalidOperationException($"Could not resolve handler for {typeof(IEventHandler<TEvent, TContext>).FullName}");

			return handlers;
		}

		public override IResult Handle(
			IEvent vent,
			IEventHandlerContext handlerContext,
			IServiceProvider serviceProvider)
			=> Handle((TEvent)vent, (TContext)handlerContext, serviceProvider);

		public IResult Handle(
			TEvent vent,
			TContext handlerContext,
			IServiceProvider serviceProvider)
		{
			List<IEventHandler<TEvent, TContext>>? handlers = null;
			try
			{
				handlers = CreateHandlers(serviceProvider).Select(x => (IEventHandler<TEvent, TContext>)x).ToList();
			}
			catch (Exception exHandler)
			{
				handlerContext.LogError(TraceInfo.Create(handlerContext.PreviousTraceInfo), MessageStatus.Aborted, x => x.ExceptionInfo(exHandler), $"PublishAsync<IEvent> {nameof(CreateHandlers)} error", null);
				throw;
			}

			var resultBuilder = new ResultBuilder();
			IResult? result = null;
			foreach (var handler in handlers)
			{
				try
				{
					var interceptorType = handler.InterceptorType;
					if (interceptorType == null)
					{
						result = handler.Handle(vent, handlerContext);
					}
					else
					{
						var interceptor = (IEventHandlerInterceptor<TEvent, TContext>?)serviceProvider.GetService(interceptorType);
						if (interceptor == null)
							throw new InvalidOperationException($"Could not resolve interceptor for {typeof(IEventHandlerInterceptor<TEvent, TContext>).FullName}");

						result = interceptor.InterceptHandle(vent, handlerContext, handler.Handle);
					}

					resultBuilder.Merge(result);
				}
				catch (Exception exHandler)
				{
					handlerContext.LogError(TraceInfo.Create(handlerContext.PreviousTraceInfo), MessageStatus.Aborted, x => x.ExceptionInfo(exHandler), "PublishAsync<IEvent> error", null);
					throw;
				}
			}

			return resultBuilder.Build();
		}
	}
}
