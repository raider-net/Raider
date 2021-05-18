using System.Collections.Generic;

namespace Raider.Validation.Client
{
	public enum Operators
	{
		EqualsTo,
		NotEqualsTo,
		LessThan,
		LessThanOrEqualTo,
		GreaterThan,
		GreaterThanOrEqualTo,
		StartsWith,
		EndsWith,
		Contains,
		NotStartsWith,
		NotEndsWith,
		NotContains,
		IsNull,
		IsNotNull
	}

	public enum LogicalOperators
	{
		And,
		Or
	}

	public static class OperatorConverter
	{
		public static Dictionary<Operators, string> Operators = new Dictionary<Operators, string>
		{
			{ Client.Operators.EqualsTo, "=="},
			{ Client.Operators.NotEqualsTo, "!="},
			{ Client.Operators.LessThan, "<"},
			{ Client.Operators.LessThanOrEqualTo, "<="},
			{ Client.Operators.GreaterThan, ">"},
			{ Client.Operators.GreaterThanOrEqualTo, ">="},
			{ Client.Operators.StartsWith, nameof(Client.Operators.StartsWith) },
			{ Client.Operators.EndsWith, nameof(Client.Operators.EndsWith) },
			{ Client.Operators.Contains, nameof(Client.Operators.Contains) },
			{ Client.Operators.NotStartsWith, nameof(Client.Operators.NotStartsWith) },
			{ Client.Operators.NotEndsWith, nameof(Client.Operators.NotEndsWith) },
			{ Client.Operators.NotContains, nameof(Client.Operators.NotContains) },
			{ Client.Operators.IsNull, nameof(Client.Operators.IsNull) },
			{ Client.Operators.IsNotNull, nameof(Client.Operators.IsNotNull) }
		};

		public static Dictionary<LogicalOperators, string> LogicalOperators = new Dictionary<LogicalOperators, string>
		{
			{ Client.LogicalOperators.And, "&&"},
			{ Client.LogicalOperators.Or, "||"}
		};
	}
}
