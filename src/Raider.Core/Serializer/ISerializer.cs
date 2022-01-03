using System;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Serializer
{
	public interface ISerializer
	{
		string SerializeAsString<T>(T value);
		byte[] SerializeAsByteArray<T>(T value);
		T? Deserialize<T>(string payload);
		object? Deserialize(Type returnType, string payload);
		T? Deserialize<T>(byte[] payload);
		object? Deserialize(Type returnType, byte[] payload);
		bool TryDeserialize<T>(string payload, [NotNullWhen(true)] out T? value);
		bool TryDeserialize(Type returnType, string payload, [NotNullWhen(true)] out object? value);
		bool TryDeserialize<T>(byte[] payload, [NotNullWhen(true)] out T? value);
		bool TryDeserialize(Type returnType, byte[] payload, [NotNullWhen(true)] out object? value);
	}
}
