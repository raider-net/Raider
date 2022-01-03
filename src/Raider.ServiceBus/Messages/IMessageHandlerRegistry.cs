using System;

namespace Raider.ServiceBus.Messages
{
	public interface IMessageHandlerRegistry
	{
		//void RegisterAssemblyTypes(IEnumerable<Assembly> assembliesToScan);
		bool TryRegisterHandlerAndInterceptor(Type type);


		//Type? GetVoidMessageHandlerType<TRequestMessage>()
		//	where TRequestMessage : Messages.IRequestMessage;

		//Type? GetVoidMessageHandlerType(Type messageType);

		//void RegisterVoidMessageHandler<THandler>();

		//void RegisterVoidMessageHandler(Type handlerType);

		//void RegisterVoidMessageHandler<TRequestMessage, THandler, TContext>()
		//	where TRequestMessage : Messages.IRequestMessage
		//	where THandler : IMessageHandler<TRequestMessage, TContext>
		//	where TContext : IMessageHandlerContext;

		//void RegisterVoidMessageHandler(Type messageType, Type contextType, Type handlerType);


		//Type? GetAsyncVoidMessageHandlerType<TRequestMessage>()
		//	where TRequestMessage : Messages.IRequestMessage;

		//Type? GetAsyncVoidMessageHandlerType(Type messageType);

		//void RegisterAsyncVoidMessageHandler<THandler>();

		//void RegisterAsyncVoidMessageHandler(Type handlerType);

		//void RegisterAsyncVoidMessageHandler<TRequestMessage, THandler, TContext>()
		//	where TRequestMessage : Messages.IRequestMessage
		//	where THandler : IAsyncMessageHandler<TRequestMessage, TContext>
		//	where TContext : IMessageHandlerContext;

		//void RegisterAsyncVoidMessageHandler(Type messageType, Type contextType, Type handlerType);


		//Type? GetMessageHandlerType<TRequestMessage, TResponse>()
		//	where TRequestMessage : Messages.IRequestMessage<TResponse>;

		//Type? GetMessageHandlerType(Type messageType);

		//void RegisterMessageHandler<THandler>();

		//void RegisterMessageHandler(Type handlerType);

		//void RegisterMessageHandler<TRequestMessage, TResponse, THandler, TContext>()
		//	where TRequestMessage : Messages.IRequestMessage<TResponse>
		//	where THandler : IMessageHandler<TRequestMessage, TResponse, TContext>
		//	where TContext : IMessageHandlerContext;

		//void RegisterMessageHandler(Type messageType, Type resultType, Type contextType, Type handlerType);


		//Type? GetAsyncMessageHandlerType<TRequestMessage, TResponse>()
		//	where TRequestMessage : Messages.IRequestMessage<TResponse>;

		//Type? GetAsyncMessageHandlerType(Type messageType);

		//void RegisterAsyncMessageHandler<THandler>();

		//void RegisterAsyncMessageHandler(Type handlerType);

		//void RegisterAsyncMessageHandler<TRequestMessage, TResponse, THandler, TContext>()
		//	where TRequestMessage : Messages.IRequestMessage<TResponse>
		//	where THandler : IAsyncMessageHandler<TRequestMessage, TResponse, TContext>
		//	where TContext : IMessageHandlerContext;

		//void RegisterAsyncMessageHandler(Type messageType, Type resultType, Type contextType, Type handlerType);
	}
}
