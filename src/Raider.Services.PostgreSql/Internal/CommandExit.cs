using System;
using System.Collections.Generic;

namespace Raider.Services.PostgreSql
{
	public class CommandExit : Serializer.IDictionaryObject
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
