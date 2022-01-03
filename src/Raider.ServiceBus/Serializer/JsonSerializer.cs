using Raider.Serializer;
using System;
using System.Text;
using System.Text.Json;

namespace Raider.ServiceBus.Serializer
{
	public class JsonSerializer : SerializerBase, ISerializer
	{
		private readonly JsonSerializerOptions? _jsonSerializerOptions;

		public JsonSerializer()
			: this(null, UTF8Encoding)
		{
		}

		public JsonSerializer(JsonSerializerOptions? jsonSerializerOptions, Encoding encoding)
			: base(encoding)
		{
			_jsonSerializerOptions = jsonSerializerOptions;
		}

		public override string SerializeAsString<T>(T value)
		{
			object? objectValue = value;
			var jsonPayload = System.Text.Json.JsonSerializer.Serialize(objectValue, _jsonSerializerOptions);
			return jsonPayload;
		}

		public override T Deserialize<T>(string payload)
		{
			var value = System.Text.Json.JsonSerializer.Deserialize<T>(payload, _jsonSerializerOptions);
			return value!;
		}

		public override object? Deserialize(Type returnType, string payload)
		{
			var value = System.Text.Json.JsonSerializer.Deserialize(payload, returnType, _jsonSerializerOptions);
			return value;
		}
	}
}
