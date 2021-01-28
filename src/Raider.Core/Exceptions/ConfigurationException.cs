using System;

namespace Raider.Exceptions
{
	public sealed class ConfigurationException : Exception
	{
		public ConfigurationException()
			:base()
		{ }

		public ConfigurationException(string? message)
			: base(message)
		{ }

		public ConfigurationException(string? message, Exception? innerException)
			: base(message, innerException)
		{ }
	}
}
