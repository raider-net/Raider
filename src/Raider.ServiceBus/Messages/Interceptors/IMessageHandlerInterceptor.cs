using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages.Interceptors
{
	/// <summary>
	/// Defines a base interceptor for message handlers
	/// </summary>
	public interface IMessageHandlerInterceptor
	{
	}

	/// <summary>
	/// Defines an interceptor for message handlers
	/// </summary>
	/// <typeparam name="TRequestMessage">The type of request message being handled</typeparam>
	/// <typeparam name="TResponse">The type of response from the handler</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IMessageHandlerContext"/></typeparam>
	public interface IMessageHandlerInterceptor<TRequestMessage, TResponse, TContext> : IMessageHandlerInterceptor
		where TRequestMessage : Messages.IRequestMessage<TResponse>
		where TContext : IMessageHandlerContext
	{
		/// <summary>
		/// Intercepts the message handler handle method
		/// </summary>
		IResult<TResponse> InterceptHandle(TRequestMessage message, TContext handlerContext, Func<TRequestMessage, TContext, IResult<TResponse>> next);
	}

	/// <summary>
	/// Defines an interceptor for message handlers
	/// </summary>
	/// <typeparam name="TRequestMessage">The type of request message being handled</typeparam>
	/// <typeparam name="TResponse">The type of response from the handler</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IMessageHandlerContext"/></typeparam>
	public interface IAsyncMessageHandlerInterceptor<TRequestMessage, TResponse, TContext> : IMessageHandlerInterceptor
		where TRequestMessage : Messages.IRequestMessage<TResponse>
		where TContext : IMessageHandlerContext
	{
		/// <summary>
		/// Intercepts the message handler handle method
		/// </summary>
		Task<IResult<TResponse>> InterceptHandleAsync(TRequestMessage message, TContext handlerContext, Func<TRequestMessage, TContext, CancellationToken, Task<IResult<TResponse>>> next, CancellationToken cancellationToken);
	}

	/// <summary>
	/// Defines an interceptor for message handlers
	/// </summary>
	/// <typeparam name="TRequestMessage">The type of request message being handled</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IMessageHandlerContext"/></typeparam>
	public interface IMessageHandlerInterceptor<TRequestMessage, TContext> : IMessageHandlerInterceptor<TRequestMessage, VoidResponseMessage, TContext>, IMessageHandlerInterceptor
		where TRequestMessage : Messages.IRequestMessage
		where TContext : IMessageHandlerContext
	{
	}

	/// <summary>
	/// Defines an interceptor for message handlers
	/// </summary>
	/// <typeparam name="TRequestMessage">The type of request message being handled</typeparam>
	/// <typeparam name="TContext">The type of <see cref="IMessageHandlerContext"/></typeparam>
	public interface IAsyncMessageHandlerInterceptor<TRequestMessage, TContext> : IAsyncMessageHandlerInterceptor<TRequestMessage, VoidResponseMessage, TContext>, IMessageHandlerInterceptor
		where TRequestMessage : Messages.IRequestMessage
		where TContext : IMessageHandlerContext
	{
	}
}
