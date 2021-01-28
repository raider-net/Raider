using System;

namespace Raider.Trace
{
	public struct MethodParameter
	{
		public string ParameterName { get; }
		public string? SerializedValue { get; }

		public MethodParameter(string parameterName, string serializedValue)
		{
			ParameterName = string.IsNullOrWhiteSpace(parameterName)
				? throw new ArgumentNullException(nameof(parameterName))
				: parameterName;

			SerializedValue = serializedValue;
		}
	}
}
