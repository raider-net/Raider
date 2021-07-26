using Raider.Reflection.Delegates.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Raider.Reflection.Delegates
{
	/// <summary>
	///     Creates delegates for types members
	/// </summary>
	public static partial class DelegateFactory
	{
		/// <summary>
		///     Creates delegate for indexer set accessor with unspecified number of indexes from instance as object
		/// </summary>
		/// <param name="source">Type with defined indexer</param>
		/// <param name="indexTypes">Collection of index parameters types</param>
		/// <returns>Delegate for indexer set accessor with unspecified number of indexes</returns>
		public static Action<object, object[], object>? IndexerSetNew(this Type source, params Type[] indexTypes)
		{
			return source.IndexerSetObjectsImpl<Action<object, object[], object>>(indexTypes);
		}

		private static TDelegate? IndexerSetObjectsImpl<TDelegate>(this Type source,
			params Type[] indexTypes)
			where TDelegate : class
		{
			var indexerInfo = source.GetIndexerPropertyInfo(indexTypes);
			if (indexerInfo?.SetMethod == null) return null;
			ParameterExpression sourceObjectParam;
			if (source.GetTypeInfo().IsClass || source.GetTypeInfo().IsInterface)
				sourceObjectParam = Expression.Parameter(typeof(object), "source");
			else
				sourceObjectParam = Expression.Parameter(typeof(object).MakeByRefType(), "source");
			var indexesParam = Expression.Parameter(typeof(object[]), "indexes");
			var valueParam = Expression.Parameter(typeof(object), "value");
			var paramsExpression = new Expression[indexTypes.Length + 1];
			for (var i = 0; i < indexTypes.Length; i++)
			{
				var indexType = indexTypes[i];
				paramsExpression[i] = Expression.Convert(Expression.ArrayIndex(indexesParam, Expression.Constant(i)),
					indexType);
			}

			paramsExpression[indexTypes.Length] = Expression.Convert(valueParam, indexerInfo.PropertyType);
			Expression returnExpression;
			if (source.GetTypeInfo().IsClass)
			{
				returnExpression = Expression.Call(Expression.Convert(sourceObjectParam, source),
					indexerInfo.SetMethod, paramsExpression);
			}
			else
			{
				var structVariable = Expression.Variable(source, "struct");
				returnExpression = Expression.Block(typeof(object), new[] { structVariable },
					Expression.Assign(structVariable, Expression.Convert(sourceObjectParam, source)),
					Expression.Call(structVariable, indexerInfo.SetMethod, paramsExpression),
					Expression.Assign(sourceObjectParam, Expression.Convert(structVariable, typeof(object)))
				);
			}

			return Expression.Lambda<TDelegate>(returnExpression, sourceObjectParam, indexesParam, valueParam)
				.Compile();
		}
	}
}
