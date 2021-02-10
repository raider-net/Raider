using System;

namespace Raider.Validation.Client
{
	public interface IClientConditionDefinition
	{
		LogicalOperators? LogicalOperator { get; }
		string? PropertyName { get; }
		Func<object?, object?>? PropertyGetter { get; }
		Operators? Operator { get; }
		string? ValuePropertyName { get; }
		Func<object?, object?>? ValueGetter { get; }
		IClientConditionDefinition? Left { get; }
		IClientConditionDefinition? Right { get; }

		bool Execute(object? obj);
	}
}
