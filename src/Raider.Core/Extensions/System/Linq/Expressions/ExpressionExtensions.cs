using Raider.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Raider.Extensions
{
	public static class ExpressionExtensions
	{
		private class ParameterTypeVisitor<TSource, TTarget> : ExpressionVisitor
		{
			private ReadOnlyCollection<ParameterExpression>? _parameters;

			protected override Expression VisitParameter(ParameterExpression node)
			{
				return _parameters?.FirstOrDefault(p => p.Name == node.Name) ??
					(node.Type == typeof(TSource) ? Expression.Parameter(typeof(TTarget), node.Name) : node);
			}

			protected override Expression VisitLambda<T>(Expression<T> node)
			{
				_parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");
				return Expression.Lambda(Visit(node.Body), _parameters);
			}

			protected override Expression VisitMember(MemberExpression node)
			{
				if (node.Member.DeclaringType == typeof(TSource))
				{
					return Expression.Property(Visit(node.Expression), node.Member.Name);
				}
				return base.VisitMember(node);
			}
		}

		private class ReplaceExpressionVisitor : ExpressionVisitor
		{
			private readonly Expression _oldValue;
			private readonly Expression _newValue;

			public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
			{
				_oldValue = oldValue;
				_newValue = newValue;
			}

			public override Expression? Visit(Expression? node)
			{
				if (node == _oldValue)
					return _newValue;
				return base.Visit(node);
			}
		}

		public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
		{
			if (expr1 == null)
			{
				return expr2;
			}
			if (expr2 == null)
			{
				return expr1;
			}

			var parameter = Expression.Parameter(typeof(T));

			var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
			var left = leftVisitor.Visit(expr1.Body);

			var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
			var right = rightVisitor.Visit(expr2.Body);

			return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
		}

		public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
		{
			if (expr1 == null)
			{
				return expr2;
			}
			if (expr2 == null)
			{
				return expr1;
			}

			var parameter = Expression.Parameter(typeof(T));

			var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
			var left = leftVisitor.Visit(expr1.Body);

			var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
			var right = rightVisitor.Visit(expr2.Body);

			return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left, right), parameter);
		}

		public static Expression<Func<TTarget, bool>> Convert<TSource, TTarget>(this Expression<Func<TSource, bool>> root)
		{
			var visitor = new ParameterTypeVisitor<TSource, TTarget>();
			Expression<Func<TTarget, bool>> expression = (Expression<Func<TTarget, bool>>)visitor.Visit(root);
			return expression;
		}

		public static MemberInfo GetMemberInfo<T>(this Expression<Func<T, object>> expression) //where T : class
		{
			return ReflectionHelper.GetMemberInfo(expression);
		}

		public static MemberInfo GetMemberInfo<T, E>(this Expression<Func<T, E>> expression) //where T : class
		{
			return ReflectionHelper.GetMemberInfo(expression);
		}

		public static string GetMemberName<T>(this Expression<Func<T, object?>> expression) //where T : class
		{
			return ReflectionHelper.GetMemberName(expression);
		}

		public static string GetMemberName<T, E>(this Expression<Func<T, E>> expression) //where T : class
		{
			return ReflectionHelper.GetMemberName(expression);
		}

		public static string GetMemberName<T, E>(this Expression<Func<T, E>> expression, bool getFullPropertyPath) //where T : class
		{
			return ReflectionHelper.GetMemberName(expression, getFullPropertyPath);
		}

		/// <example>
		/// pre x => x.ParentChild2List[0].Child2Name, removeMetohds = false
		/// vrati ParentChild2List, get_Item(0), Child2Name
		/// 
		/// pre x => x.ParentChild2List[0].Child2Name, removeMetohds = true
		/// vrati ParentChild2List, Child2Name
		/// 
		/// pre x => x.ParentChild2Enumerable.First().Child2Name, removeMetohds = false
		/// vrati ParentChild2Enumerable, First(), Child2Name
		/// 
		/// pre x => x.ParentChild2Enumerable.First().Child2Name, removeMetohds = true
		/// vrati ParentChild2Enumerable, Child2Name
		/// </example>
		public static List<string> GetMemberNamePath<T, E>(this Expression<Func<T, E>> expression, bool removeMetohds = false) //where T : class
		{
			return ReflectionHelper.GetMemberNamePath(expression, removeMetohds);
		}

		public static Type GetFieldOrPropertyType<T>(this Expression<Func<T, object>> expression) //where T : class
		{
			return ReflectionHelper.GetFieldOrPropertyType(expression);
		}

		public static Type GetFieldOrPropertyType<T, E>(this Expression<Func<T, E>> expression) //where T : class
		{
			return ReflectionHelper.GetFieldOrPropertyType(expression);
		}

		////TODO
		//public static Func<T, E> ToFunc<T, E>(this Expression<Func<T, E>> expression)
		//{
		//	//TODO: from FastExpressionCompiler.dll
		//	return expression?.CompileFast();
		//}
	}
}
