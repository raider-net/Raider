using Raider.ServiceBus.Model;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Internal.Model
{
	public class DbMessageType : IMessageType, Raider.Serializer.IDictionaryObject
	{
		public Guid IdMessageType { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public int IdMessageMetaType { get; set; }
		public string CrlType { get; set; }

		public DbMessageType(Guid id, string name, int idMessageTypeMetadata, string crlType)
		{
			IdMessageType = id;
			Name = name;
			IdMessageMetaType = idMessageTypeMetadata;
			CrlType = crlType;
		}

		public IDictionary<string, object?> ToDictionary(Raider.Serializer.ISerializer? serializer = null)
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(IdMessageType), IdMessageType },
				{ nameof(Name), Name },
				{ nameof(IdMessageMetaType), IdMessageMetaType },
				{ nameof(CrlType), CrlType }
			};

			if (string.IsNullOrWhiteSpace(Description))
				dict.Add(nameof(Description), Description);

			return dict;
		}
	}
}
