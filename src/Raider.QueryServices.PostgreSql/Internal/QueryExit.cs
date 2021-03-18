using System;
using System.Collections.Generic;

namespace Raider.QueryServices.PostgreSql
{
	public class QueryExit : Serializer.IDictionaryObject
	{
		public Guid IdCommandQueryEntry { get; set; }
		public decimal ElapsedMilliseconds { get; set; }

		public IDictionary<string, object?> ToDictionary()
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(IdCommandQueryEntry), IdCommandQueryEntry },
				{ nameof(ElapsedMilliseconds), ElapsedMilliseconds }
			};

			return dict;
		}
	}
}
