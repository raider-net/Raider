using Raider.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Raider.Validation.Internal
{
	internal static class PropertyAccessor
	{
		private static readonly ConcurrentDictionary<Key, Delegate> _cache = new ConcurrentDictionary<Key, Delegate>();

		public static Func<T, TProperty> GetCachedAccessor<T, TProperty>(Expression<Func<T, TProperty>> expression)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			var memberInfo = expression.GetMemberInfo();
			var key = new Key(typeof(T), typeof(TProperty), memberInfo);

			return (Func<T, TProperty>)_cache.GetOrAdd(key, k => expression.Compile());
		}

		private class Key
		{
			private readonly Type _reflectedType;
			private readonly Type _expressionPropertyType;
			private readonly MemberInfo _memberInfo;

			public Key(Type reflectedType, Type expressionPropertyType, MemberInfo member)
			{
				_reflectedType = reflectedType;
				_expressionPropertyType = expressionPropertyType;
				_memberInfo = member;
			}

			protected bool Equals(Key other)
			{
				return Equals(_memberInfo, other._memberInfo)
					&& Equals(_reflectedType, other._reflectedType)
					&& Equals(_expressionPropertyType, other._expressionPropertyType);
			}

			public override bool Equals(object? obj)
			{
				if (obj is null) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((Key)obj);
			}

			public override int GetHashCode()
				=> HashCode.Combine(_reflectedType, _expressionPropertyType, _memberInfo);
		}
	}
}
