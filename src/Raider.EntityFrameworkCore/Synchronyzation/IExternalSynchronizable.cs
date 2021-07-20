//using System;

//namespace Raider.EntityFrameworkCore.Synchronyzation
//{
//	public interface IExternalSynchronizable
//	{
//		long IdSync { get; set; }
//		Guid SyncToken { get; set; }
//		object InternalId { get; set; }
//		DateTime? SentToExternalSystemAt { get; set; }
//		DateTime? ReceivedFromExternalSystemAt { get; set; }
//		object? ExternalId { get; set; }
//		DateTime? ConfirmedDeliveryAt { get; set; }
//		int IdExternalSystem { get; set; }
//	}

//	public interface IExternalSynchronizable<T> : IExternalSynchronizable
//		where T : struct
//	{
//		new T InternalId { get; set; }
//		new T? ExternalId { get; set; }
//	}
//}
