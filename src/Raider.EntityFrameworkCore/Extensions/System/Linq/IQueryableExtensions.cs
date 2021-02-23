using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Raider.Extensions
{
	public static class IQueryableExtensions
	{
		public static Task<bool> AnyAsyncSafe<TSource>(this IQueryable<TSource> query, Expression<Func<TSource, bool>> predicate)
		{
			if (query == null)
				return Task.FromResult(false);
			if (predicate == null)
				return query.AnyAsync();

			return query.AnyAsync(predicate);
		}

		public static Task<bool> AllAsyncSafe<TSource>(this IQueryable<TSource> query, Expression<Func<TSource, bool>> predicate)
		{
			if (query == null || predicate == null)
				return Task.FromResult(false);

			return query.AllAsync(predicate);
		}
	}
}
