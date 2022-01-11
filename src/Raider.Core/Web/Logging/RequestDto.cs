using Raider.Infrastructure;
using System;
using System.Collections.Generic;

namespace Raider.Web.Logging
{
	public class RequestDto : Serializer.IDictionaryObject
	{
		public Guid RuntimeUniqueKey { get; set; }
		public DateTimeOffset Created { get; set; }
		public Guid? CorrelationId { get; set; }
		public string? ExternalCorrelationId { get; set; }
		public string? Protocol { get; set; }
		public string? Scheme { get; set; }
		public string? Host { get; set; }
		public string? RemoteIp { get; set; }
		public string? Method { get; set; }
		public string? Path { get; set; }
		public string? QueryString { get; set; }
		public string? Headers { get; set; }
		public string? Body { get; set; }
		public byte[]? BodyByteArray { get; set; }
		public string? Form { get; set; }
		public string? Files { get; set; }

		public RequestDto()
		{
			RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
			Created = DateTimeOffset.Now;
		}

		public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
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

			if (!string.IsNullOrWhiteSpace(Protocol))
				dict.Add(nameof(Protocol), Protocol);

			if (!string.IsNullOrWhiteSpace(Scheme))
				dict.Add(nameof(Scheme), Scheme);

			if (!string.IsNullOrWhiteSpace(Host))
				dict.Add(nameof(Host), Host);

			if (!string.IsNullOrWhiteSpace(RemoteIp))
				dict.Add(nameof(RemoteIp), RemoteIp);

			if (!string.IsNullOrWhiteSpace(Method))
				dict.Add(nameof(Method), Method);

			if (!string.IsNullOrWhiteSpace(Path))
				dict.Add(nameof(Path), Path);

			if (!string.IsNullOrWhiteSpace(QueryString))
				dict.Add(nameof(QueryString), QueryString);

			if (!string.IsNullOrWhiteSpace(Headers))
				dict.Add(nameof(Headers), Headers);

			if (!string.IsNullOrWhiteSpace(Body))
				dict.Add(nameof(Body), Body);

			if (BodyByteArray != null)
				dict.Add(nameof(BodyByteArray), BodyByteArray);

			if (!string.IsNullOrWhiteSpace(Form))
				dict.Add(nameof(Form), Form);

			if (!string.IsNullOrWhiteSpace(Files))
				dict.Add(nameof(Files), Files);

			return dict;
		}
	}
}
