using System;
using System.Collections.Generic;

namespace Raider.QueryServices.PostgreSql
{
	public class QueryExit : Serializer.IDictionaryObject
	{
		public Guid IdCommandQueryExit { get; set; }
		public decimal ElapsedMilliseconds { get; set; }
		public bool IsError { get; set; }
		public string? Data { get; set; }

		public IDictionary<string, object?> ToDictionary()
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(IdCommandQueryExit), IdCommandQueryExit },
				{ nameof(ElapsedMilliseconds), ElapsedMilliseconds },
				{ nameof(IsError), IsError },
				{ nameof(Data), Data }
			};

			return dict;
		}
	}
}
