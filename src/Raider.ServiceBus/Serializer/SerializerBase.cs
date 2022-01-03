using Raider.Serializer;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Raider.ServiceBus.Serializer
{
	public abstract class SerializerBase : ISerializer
	{
		public static readonly Encoding UTF8Encoding = new UTF8Encoding(false);

		private readonly Encoding _encoding;

		public SerializerBase()
			: this(UTF8Encoding)
		{
		}

		public SerializerBase(Encoding encoding)
		{
			_encoding = encoding ?? UTF8Encoding;
		}

		public abstract string SerializeAsString<T>(T value);

		public virtual byte[] SerializeAsByteArray<T>(T value)
		{
			var jsonPayload = SerializeAsString(value);
			var payload = _encoding.GetBytes(jsonPayload);
			return payload;
		}

		public abstract T? Deserialize<T>(string payload);

		public abstract object? Deserialize(Type returnType, string payload);

		public virtual T? Deserialize<T>(byte[] payload)
		{
			var jsonPayload = _encoding.GetString(payload);
			return Deserialize<T>(jsonPayload);
		}

		public virtual object? Deserialize(Type returnType, byte[] payload)
		{
			var jsonPayload = _encoding.GetString(payload);
			return Deserialize(returnType, jsonPayload);
		}

		public virtual bool TryDeserialize<T>(string payload, [NotNullWhen(true)] out T? value)
		{
			try
			{
				value = Deserialize<T>(payload);
				return value != null;
			}
			catch
			{
				value = default;
				return false;
			}
		}

		public virtual bool TryDeserialize(Type returnType, string payload, [NotNullWhen(true)] out object? value)
		{
			try
			{
				value = Deserialize(returnType, payload);
				return value != null;
			}
			catch
			{
				value = default;
				return false;
			}
		}

		public virtual bool TryDeserialize<T>(byte[] payload, [NotNullWhen(true)] out T? value)
		{
			try
			{
				value = Deserialize<T>(payload);
				return value != null;
			}
			catch
			{
				value = default;
				return false;
			}
		}

		public virtual bool TryDeserialize(Type returnType, byte[] payload, [NotNullWhen(true)] out object? value)
		{
			try
			{
				value = Deserialize(returnType, payload);
				return value != null;
			}
			catch
			{
				value = default;
				return false;
			}
		}
	}
}
