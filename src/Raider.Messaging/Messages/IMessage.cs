﻿using System;

namespace Raider.Messaging.Messages
{
	public interface IMessage
	{
		Guid IdMessage { get; }
		Guid? IdPreviousMessage { get; }
		Guid BusinessId { get; }
		int IdScenario { get; }
		int IdPublisher { get; }
		DateTimeOffset CreatedUtc { get; }
		bool IsRecovery { get; }
		bool HasData { get; }
		object? GetData();
	}

	public interface IMessage<TData> : IMessage
		where TData : IMessageData
	{
		bool IMessage.HasData => true;

		object? IMessage.GetData()
			=> Data;

		TData? Data { get; }
	}
}
