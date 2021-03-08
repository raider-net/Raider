using Raider.Infrastructure;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.Dto
{
	internal class RequestAuthentication : Serializer.IDictionaryObject
	{
		public Guid RuntimeUniqueKey { get; set; }
		public DateTimeOffset Created { get; set; }
		public Guid? CorrelationId { get; set; }
		public string? ExternalCorrelationId { get; set; }
		public int? IdUser { get; set; }
		public string? Roles { get; set; }
		public string? Permissions { get; set; }

		public RequestAuthentication()
		{
			RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
			Created = DateTimeOffset.Now;
		}

		public IReadOnlyDictionary<string, object?> ToDictionary()
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
				{ nameof(Created), Created },
			};

			if (CorrelationId.HasValue)
				dict.Add(nameof(CorrelationId), CorrelationId);

			if (!string.IsNullOrWhiteSpace(ExternalCorrelationId))
				dict.Add(nameof(ExternalCorrelationId), ExternalCorrelationId);

			if (IdUser.HasValue)
				dict.Add(nameof(IdUser), IdUser);

			if (!string.IsNullOrWhiteSpace(Roles))
				dict.Add(nameof(Roles), Roles);

			if (!string.IsNullOrWhiteSpace(Permissions))
				dict.Add(nameof(Permissions), Permissions);

			return dict;
		}
	}
}
