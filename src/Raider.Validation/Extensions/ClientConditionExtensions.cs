using Raider.Extensions;
using Raider.Validation.Client;
using Raider.Validation.Internal;
using System;
using System.Linq.Expressions;

namespace Raider.Validation
{
	public static class ClientConditionExtensions
	{
		private static ClientCondition<T> SetClientCondition<T, TProperty>(
			ClientCondition<T> clientCondition,
			Operators @operator,
			Expression<Func<T, TProperty?>> property,
			TProperty? value)
		{
			clientCondition.LogicalOperator = null;
			clientCondition.PropertyName = property.GetMemberName(true);
			clientCondition.PropertyGetter = PropertyAccessor.GetCachedAccessor(property).ToNonGeneric();
			clientCondition.Operator = @operator;
			clientCondition.ValuePropertyName = null;
			clientCondition.ValueGetter = x => value;
			clientCondition.Left = null;
			clientCondition.Right = null;
			return clientCondition;
		}

		private static ClientCondition<T> SetClientCondition<T, TProperty>(
			ClientCondition<T> clientCondition,
			Operators @operator,
			Expression<Func<T, TProperty?>> property,
			Expression<Func<T, TProperty?>> expression)
		{
			clientCondition.LogicalOperator = null;
			clientCondition.PropertyName = property.GetMemberName(true);
			clientCondition.PropertyGetter = PropertyAccessor.GetCachedAccessor(property).ToNonGeneric();
			clientCondition.Operator = @operator;
			clientCondition.ValuePropertyName = expression.GetMemberName(true);
			clientCondition.ValueGetter = PropertyAccessor.GetCachedAccessor(expression).ToNonGeneric();
			clientCondition.Left = null;
			clientCondition.Right = null;
			return clientCondition;
		}

		public static IClientConditionDefinition EqualsTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty? value)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.EqualsTo, property, value);

		public static IClientConditionDefinition EqualsTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty? value)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.EqualsTo, property, value);

		public static IClientConditionDefinition EqualsTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.EqualsTo, property, expression);

		public static IClientConditionDefinition EqualsTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.EqualsTo, property, expression);

		public static IClientConditionDefinition NotEqualsTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.NotEqualsTo, property, value);

		public static IClientConditionDefinition NotEqualsTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.NotEqualsTo, property, value);

		public static IClientConditionDefinition NotEqualsTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.NotEqualsTo, property, expression);

		public static IClientConditionDefinition NotEqualsTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.NotEqualsTo, property, expression);

		public static IClientConditionDefinition LessThan<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.LessThan, property, value);

		public static IClientConditionDefinition LessThan<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.LessThan, property, value);

		public static IClientConditionDefinition LessThan<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.LessThan, property, expression);

		public static IClientConditionDefinition LessThan<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.LessThan, property, expression);

		public static IClientConditionDefinition LessThanOrEqualTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.LessThanOrEqualTo, property, value);

		public static IClientConditionDefinition LessThanOrEqualTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.LessThanOrEqualTo, property, value);

		public static IClientConditionDefinition LessThanOrEqualTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.LessThanOrEqualTo, property, expression);

		public static IClientConditionDefinition LessThanOrEqualTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.LessThanOrEqualTo, property, expression);

		public static IClientConditionDefinition GreaterThan<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.GreaterThan, property, value);

		public static IClientConditionDefinition GreaterThan<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.GreaterThan, property, value);

		public static IClientConditionDefinition GreaterThan<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.GreaterThan, property, expression);

		public static IClientConditionDefinition GreaterThan<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.GreaterThan, property, expression);

		public static IClientConditionDefinition GreaterThanOrEqualTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.GreaterThanOrEqualTo, property, value);

		public static IClientConditionDefinition GreaterThanOrEqualTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, TProperty value)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.GreaterThanOrEqualTo, property, value);

		public static IClientConditionDefinition GreaterThanOrEqualTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : struct, IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.GreaterThanOrEqualTo, property, expression);

		public static IClientConditionDefinition GreaterThanOrEqualTo<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property, Expression<Func<T, TProperty?>> expression)
			where TProperty : IComparable<TProperty>, IComparable
			=> SetClientCondition(clientCondition, Operators.GreaterThanOrEqualTo, property, expression);

		public static IClientConditionDefinition StartsWith<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, string value)
			=> SetClientCondition(clientCondition, Operators.StartsWith, property, value);

		public static IClientConditionDefinition StartsWith<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, Expression<Func<T, string?>> expression)
			=> SetClientCondition(clientCondition, Operators.StartsWith, property, expression);

		public static IClientConditionDefinition EndsWith<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, string value)
			=> SetClientCondition(clientCondition, Operators.EndsWith, property, value);

		public static IClientConditionDefinition EndsWith<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, Expression<Func<T, string?>> expression)
			=> SetClientCondition(clientCondition, Operators.EndsWith, property, expression);

		public static IClientConditionDefinition Contains<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, string value)
			=> SetClientCondition(clientCondition, Operators.Contains, property, value);

		public static IClientConditionDefinition Contains<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, Expression<Func<T, string?>> expression)
			=> SetClientCondition(clientCondition, Operators.Contains, property, expression);

		public static IClientConditionDefinition NotStartsWith<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, string value)
			=> SetClientCondition(clientCondition, Operators.NotStartsWith, property, value);

		public static IClientConditionDefinition NotStartsWith<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, Expression<Func<T, string?>> expression)
			=> SetClientCondition(clientCondition, Operators.NotStartsWith, property, expression);

		public static IClientConditionDefinition NotEndsWith<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, string value)
			=> SetClientCondition(clientCondition, Operators.NotEndsWith, property, value);

		public static IClientConditionDefinition NotEndsWith<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, Expression<Func<T, string?>> expression)
			=> SetClientCondition(clientCondition, Operators.NotEndsWith, property, expression);

		public static IClientConditionDefinition NotContains<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, string value)
			=> SetClientCondition(clientCondition, Operators.NotContains, property, value);

		public static IClientConditionDefinition NotContains<T>(this ClientCondition<T> clientCondition, Expression<Func<T, string?>> property, Expression<Func<T, string?>> expression)
			=> SetClientCondition(clientCondition, Operators.NotContains, property, expression);

		public static IClientConditionDefinition IsNull<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property)
			=> SetClientCondition(clientCondition, Operators.IsNull, property, (TProperty?)default);

		public static IClientConditionDefinition IsNotNull<T, TProperty>(this ClientCondition<T> clientCondition, Expression<Func<T, TProperty?>> property)
			=> SetClientCondition(clientCondition, Operators.IsNotNull, property, (TProperty?)default);

		public static IClientConditionDefinition And<T>(this ClientCondition<T> clientCondition, Action<ClientCondition<T>> leftCondition, Action<ClientCondition<T>> rightCondition)
		{
			if (leftCondition == null)
				throw new ArgumentNullException(nameof(leftCondition));
			if (rightCondition == null)
				throw new ArgumentNullException(nameof(rightCondition));

			clientCondition.LogicalOperator = LogicalOperators.And;
			clientCondition.PropertyName = null;
			clientCondition.PropertyGetter = null;
			clientCondition.Operator = null;
			clientCondition.ValuePropertyName = null;
			clientCondition.ValueGetter = null;

			var left = new ClientCondition<T>();
			leftCondition.Invoke(left);

			var right = new ClientCondition<T>();
			rightCondition.Invoke(right);

			clientCondition.Left = left;
			clientCondition.Right = right;
			return clientCondition;
		}

		public static IClientConditionDefinition Or<T>(this ClientCondition<T> clientCondition, Action<ClientCondition<T>> leftCondition, Action<ClientCondition<T>> rightCondition)
		{
			if (leftCondition == null)
				throw new ArgumentNullException(nameof(leftCondition));
			if (rightCondition == null)
				throw new ArgumentNullException(nameof(rightCondition));

			clientCondition.LogicalOperator = LogicalOperators.Or;
			clientCondition.PropertyName = null;
			clientCondition.PropertyGetter = null;
			clientCondition.Operator = null;
			clientCondition.ValuePropertyName = null;
			clientCondition.ValueGetter = null;

			var left = new ClientCondition<T>();
			leftCondition.Invoke(left);

			var right = new ClientCondition<T>();
			rightCondition.Invoke(right);

			clientCondition.Left = left;
			clientCondition.Right = right;
			return clientCondition;
		}
	}
}
