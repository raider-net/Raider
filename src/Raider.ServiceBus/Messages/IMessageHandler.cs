﻿using Raider.ServiceBus.Messages.Interceptors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages
{
	/// <summary>
	/// Defines a base handler for a request messages
	/// </summary>
	public interface IMessageHandler
	{
	}

	/// <summary>
	/// Defines a handler for a request message with response message
	/// </summary>
	/// <typeparam name="TRequestMessage">The type of request message being handled</typeparam>
	/// <typeparam name="TResponse">The type of response from the handler</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IMessageHandlerContext"/></typeparam>
	public interface IMessageHandler<TRequestMessage, TResponse, TContext> : IMessageHandler
		where TRequestMessage : Messages.IRequestMessage<TResponse>
		where TContext : IMessageHandlerContext
	{
		/// <summary>
		/// Interceptor for handle method. Interceptor must implement <see cref="IMessageHandlerInterceptor{TRequestMessage, TResponse, TContext}"/>
		/// </summary>
		Type? InterceptorType { get; set; }

		/// <summary>
		/// Handles a request message
		/// </summary>
		/// <returns>Response from the request message</returns>
		IResult<TResponse> Handle(TRequestMessage message, TContext handlerContext);
	}

	/// <summary>
	/// Defines a handler for a request message with response message
	/// </summary>
	/// <typeparam name="TRequestMessage">The type of request message being handled</typeparam>
	/// <typeparam name="TResponse">The type of response from the handler</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IMessageHandlerContext"/></typeparam>
	public interface IAsyncMessageHandler<TRequestMessage, TResponse, TContext> : IMessageHandler
		where TRequestMessage : Messages.IRequestMessage<TResponse>
		where TContext : IMessageHandlerContext
	{
		/// <summary>
		/// Interceptor for handle method. Interceptor must implement <see cref="IAsyncMessageHandlerInterceptor{TRequestMessage, TResponse, TContext}"/>
		/// </summary>
		Type? InterceptorType { get; set; }

		/// <summary>
		/// Handles a request message
		/// </summary>
		/// <returns>Response from the request message</returns>
		Task<IResult<TResponse>> HandleAsync(TRequestMessage message, TContext handlerContext, CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// Defines a handler for a request message with no response message
	/// </summary>
	/// <typeparam name="TRequestMessage">The type of request message being handled</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IMessageHandlerContext"/></typeparam>
	public interface IMessageHandler<TRequestMessage, TContext> : IMessageHandler<TRequestMessage, VoidResponseMessage, TContext>, IMessageHandler
		where TRequestMessage : Messages.IRequestMessage
		where TContext : IMessageHandlerContext
	{
	}

	/// <summary>
	/// Defines a handler for a request message with no response message
	/// </summary>
	/// <typeparam name="TRequestMessage">The type of request message being handled</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IMessageHandlerContext"/></typeparam>
	public interface IAsyncMessageHandler<TRequestMessage, TContext> : IAsyncMessageHandler<TRequestMessage, VoidResponseMessage, TContext>, IMessageHandler
		where TRequestMessage : Messages.IRequestMessage
		where TContext : IMessageHandlerContext
	{
	}
}
