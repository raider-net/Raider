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
			var key = new Key(memberInfo, typeof(TProperty));

			return (Func<T, TProperty>)_cache.GetOrAdd(key, k => expression.Compile());
		}

		private class Key
		{
			private readonly MemberInfo _memberInfo;
			private readonly Type _expressionPropertyType;

			public Key(MemberInfo member, Type expressionPropertyType)
			{
				_memberInfo = member;
				_expressionPropertyType = expressionPropertyType;
			}

			protected bool Equals(Key other)
			{
				return Equals(_memberInfo, other._memberInfo) && string.Equals(_expressionPropertyType, other._expressionPropertyType);
			}

			public override bool Equals(object? obj)
			{
				if (obj is null) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((Key)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((_memberInfo != null ? _memberInfo.GetHashCode() : 0) * 397) ^ (_expressionPropertyType != null ? _expressionPropertyType.GetHashCode() : 0);
				}
			}
		}
	}
}
