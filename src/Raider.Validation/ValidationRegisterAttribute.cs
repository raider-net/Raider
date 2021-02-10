using System;

namespace Raider.Validation
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class ValidationRegisterAttribute : Attribute
	{
		public Type[] CommandTypes { get; }

		public ValidationRegisterAttribute(params Type[] commandTypes)
		{
			if (commandTypes == null)
				throw new ArgumentNullException(nameof(commandTypes));

			if (commandTypes.Length == 0)
				throw new ArgumentException("No commandType set.", nameof(commandTypes));

			CommandTypes = commandTypes;
		}
	}
}
