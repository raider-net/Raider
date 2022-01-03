﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Events.Interceptors
{
	/// <summary>
	/// Defines a base interceptor for event handlers
	/// </summary>
	public interface IEventHandlerInterceptor
	{
	}

	/// <summary>
	/// Defines an interceptor for event handlers
	/// </summary>
	/// <typeparam name="TEvent">The type of event being handled</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IEventHandlerContext"/></typeparam>
	public interface IEventHandlerInterceptor<TEvent, TContext> : IEventHandlerInterceptor
		where TEvent : IEvent
		where TContext : IEventHandlerContext
	{
		/// <summary>
		/// Intercepts the event handler handle method
		/// </summary>
		IResult InterceptHandle(TEvent @event, TContext handlerContext, Func<TEvent, TContext, IResult> next);
	}

	/// <summary>
	/// Defines an interceptor for event handlers
	/// </summary>
	/// <typeparam name="TEvent">The type of request event being handled</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IEventHandlerContext"/></typeparam>
	public interface IAsyncEventHandlerInterceptor<TEvent, TContext> : IEventHandlerInterceptor
		where TEvent : IEvent
		where TContext : IEventHandlerContext
	{
		/// <summary>
		/// Intercepts the event handler handle method
		/// </summary>
		Task<IResult> InterceptHandleAsync(TEvent @event, TContext handlerContext, Func<TEvent, TContext, CancellationToken, Task<IResult>> next, CancellationToken cancellationToken);
	}
}
