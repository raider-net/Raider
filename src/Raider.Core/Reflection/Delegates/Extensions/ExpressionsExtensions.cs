using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Raider.Reflection.Delegates.Extensions
{
	/// <summary>
	///     Expression extension methods class
	/// </summary>
	internal static class ExpressionsExtensions
	{
		/// <summary>
		///     Casts list of <see cref="ParameterExpression" /> with instance parameter
		///     <see cref="ParameterExpression" /> to .NET framework version collection compatible with second parameter
		///     of
		///     <see
		///         cref="Expression.Lambda{TDelegate}(System.Linq.Expressions.Expression,System.Collections.Generic.IEnumerable{ParameterExpression})" />
		///     method.
		/// </summary>
		/// <param name="parameters">Collection of <see cref="ParameterExpression" /></param>
		/// <param name="sourceParam">Source instance <see cref="ParameterExpression" /></param>
		/// <returns>
		///     Collection compatible with method
		///     <see
		///         cref="Expression.Lambda{TDelegate}(System.Linq.Expressions.Expression,System.Collections.Generic.IEnumerable{ParameterExpression})" />
		///     second parameter.
		/// </returns>
		public static IEnumerable<ParameterExpression> GetLambdaExprParams(
			this IEnumerable<ParameterExpression> parameters, ParameterExpression sourceParam)
		{
			return new[] { sourceParam }.Concat(parameters);
		}
	}
}
