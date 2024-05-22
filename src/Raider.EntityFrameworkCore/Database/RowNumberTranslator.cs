using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Raider.EntityFrameworkCore.Database
{
	public static class SqlServerDbContextOptionsBuilderExtensions
	{
		public static DbContextOptionsBuilder AddRowNumberSupport(this DbContextOptionsBuilder sqlServerOptionsBuilder)
		{
			var builder = (IDbContextOptionsBuilderInfrastructure)sqlServerOptionsBuilder;

			// if the extension is registered already then we keep it 
			// otherwise we create a new one

			var extension = sqlServerOptionsBuilder.Options.FindExtension<RelationalDbContextOptionsExtension>()
				?? new RelationalDbContextOptionsExtension();

			builder.AddOrUpdateExtension(extension);
			return sqlServerOptionsBuilder;
		}
	}

	/// <summary>
	/// Plugin registering method translators.
	/// </summary>
	public sealed class RelationalMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
	{
		/// <inheritdoc />
		public IEnumerable<IMethodCallTranslator> Translators { get; }

		/// <summary>
		/// Initializes new instance of <see cref="RelationalMethodCallTranslatorPlugin"/>.
		/// </summary>
		public RelationalMethodCallTranslatorPlugin()
		{
			Translators = new List<IMethodCallTranslator>
			{
				new RowNumberTranslator()
			};
		}
	}

	public sealed class RelationalDbContextOptionsExtension : IDbContextOptionsExtension
	{
		private DbContextOptionsExtensionInfo? _info;
		public DbContextOptionsExtensionInfo Info => _info ??= new RelationalDbContextOptionsExtensionInfo(this);

		/// <inheritdoc />
		public void ApplyServices(IServiceCollection services)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));

			services.TryAddSingleton(this);

			var serviceType = typeof(IMethodCallTranslatorPlugin);
			ServiceLifetime lifetime;

#pragma warning disable EF1001 // Internal EF Core API usage.
			if (EntityFrameworkRelationalServicesBuilder.RelationalServices.TryGetValue(serviceType, out var serviceCharacteristics) ||
				EntityFrameworkServicesBuilder.CoreServices.TryGetValue(serviceType, out serviceCharacteristics))
				lifetime = serviceCharacteristics.Lifetime;
			else
				throw new InvalidOperationException($"No service characteristics for service '{serviceType.Name}' found.");
#pragma warning restore EF1001 // Internal EF Core API usage.

			services.Add(ServiceDescriptor.Describe(serviceType, typeof(RelationalMethodCallTranslatorPlugin), lifetime));
		}

		/// <inheritdoc />
		public void Validate(IDbContextOptions options)
		{
		}

		private class RelationalDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
		{
			public override bool IsDatabaseProvider => false;

			private string? _logFragment;

			public override string LogFragment => _logFragment ??= "";

			public RelationalDbContextOptionsExtensionInfo(RelationalDbContextOptionsExtension extension)
				: base(extension)
			{
			}

			public override int GetServiceProviderHashCode()
			{
				var hashCode = new HashCode();
				hashCode.Add(true);

				return hashCode.ToHashCode();
			}

			public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
			{
			}

			public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
				=> true;
		}
	}

	public static class ServiceDescriptorHelper
	{
		public static int GetHashCode(ServiceDescriptor descriptor)
		{
			int implHashcode;

			if (descriptor.ImplementationType != null)
			{
				implHashcode = descriptor.ImplementationType.GetHashCode();
			}
			else if (descriptor.ImplementationInstance != null)
			{
				implHashcode = descriptor.ImplementationInstance.GetHashCode();
			}
			else
			{
				implHashcode =
					descriptor.ImplementationFactory?.GetHashCode()
						?? throw new ArgumentException("The service descriptor has no ImplementationType, ImplementationInstance and ImplementationFactory.");
			}

			return HashCode.Combine(descriptor.Lifetime, descriptor.ServiceType, implHashcode);
		}
	}

	/// <summary>
	/// Translated extension method "RowNumber"
	/// </summary>
	public sealed class RowNumberTranslator : IMethodCallTranslator
	{
		/// <inheritdoc />
		public SqlExpression? Translate(
		   SqlExpression instance,
		   MethodInfo method,
		   IReadOnlyList<SqlExpression> arguments,
		   IDiagnosticsLogger<DbLoggerCategory.Query> logger)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));
			if (arguments == null)
				throw new ArgumentNullException(nameof(arguments));

			if (method.DeclaringType != typeof(RelationalDbFunctionsExtensions))
				return null;

			switch (method.Name)
			{
				case nameof(RelationalDbFunctionsExtensions.OrderBy):
					{
						var orderBy = arguments.Skip(1).Select(e => new OrderingExpression(e, true)).ToList();
						return new RowNumberClauseOrderingsExpression(orderBy);
					}
				case nameof(RelationalDbFunctionsExtensions.OrderByDescending):
					{
						var orderBy = arguments.Skip(1).Select(e => new OrderingExpression(e, false)).ToList();
						return new RowNumberClauseOrderingsExpression(orderBy);
					}
				case nameof(RelationalDbFunctionsExtensions.ThenBy):
					{
						var orderBy = arguments.Skip(1).Select(e => new OrderingExpression(e, true));
						return ((RowNumberClauseOrderingsExpression)arguments[0]).AddColumns(orderBy);
					}
				case nameof(RelationalDbFunctionsExtensions.ThenByDescending):
					{
						var orderBy = arguments.Skip(1).Select(e => new OrderingExpression(e, false));
						return ((RowNumberClauseOrderingsExpression)arguments[0]).AddColumns(orderBy);
					}
				case nameof(RelationalDbFunctionsExtensions.RowNumber):
					{
						var partitionBy = arguments.Skip(1).Take(arguments.Count - 2).ToList();
						var orderings = (RowNumberClauseOrderingsExpression)arguments[^1];
						return new RowNumberExpression(partitionBy, orderings.Orderings, RelationalTypeMapping.NullMapping);
					}
				default:
					throw new InvalidOperationException($"Unexpected method '{method.Name}' in '{nameof(RelationalDbFunctionsExtensions)}'.");
			}
		}
	}

	public sealed class RowNumberOrderByClause
	{
		private RowNumberOrderByClause()
		{
		}
	}

	/// <summary>
	/// Accumulator for orderings.
	/// </summary>
	public sealed class RowNumberClauseOrderingsExpression : SqlExpression
	{
		/// <summary>
		/// Orderings.
		/// </summary>
		public IReadOnlyList<OrderingExpression> Orderings { get; }

		/// <inheritdoc />
		public RowNumberClauseOrderingsExpression(IReadOnlyList<OrderingExpression> orderings)
		   : base(typeof(RowNumberOrderByClause), RelationalTypeMapping.NullMapping)
		{
			Orderings = orderings ?? throw new ArgumentNullException(nameof(orderings));
		}

		/// <inheritdoc />
		protected override Expression Accept(ExpressionVisitor visitor)
		{
			if (visitor is QuerySqlGenerator)
				throw new NotSupportedException($"The EF function '{nameof(RelationalDbFunctionsExtensions.RowNumber)}' contains some expressions not supported by the Entity Framework. One of the reason is the creation of new objects like: 'new {{ e.MyProperty, e.MyOtherProperty }}'.");

			return base.Accept(visitor);
		}

		/// <inheritdoc />
		protected override Expression VisitChildren(ExpressionVisitor visitor)
		{
			var visited = visitor.VisitExpressions(Orderings);

			return ReferenceEquals(visited, Orderings) ? this : new RowNumberClauseOrderingsExpression(visited);
		}

		/// <inheritdoc />
		protected override void Print(ExpressionPrinter expressionPrinter)
		{
			if (expressionPrinter == null)
				throw new ArgumentNullException(nameof(expressionPrinter));

			expressionPrinter.VisitCollection(Orderings);
		}

		/// <summary>
		/// Adds provided <paramref name="orderings"/> to existing <see cref="Orderings"/> and returns a new <see cref="RowNumberClauseOrderingsExpression"/>.
		/// </summary>
		/// <param name="orderings">Orderings to add.</param>
		/// <returns>New instance of <see cref="RowNumberClauseOrderingsExpression"/>.</returns>
		public RowNumberClauseOrderingsExpression AddColumns(IEnumerable<OrderingExpression> orderings)
		{
			if (orderings == null)
				throw new ArgumentNullException(nameof(orderings));

			return new RowNumberClauseOrderingsExpression(Orderings.Concat(orderings).ToList());
		}

		/// <inheritdoc />
		public override bool Equals(object? obj)
		{
			return obj != null && (ReferenceEquals(this, obj) || Equals(obj as RowNumberClauseOrderingsExpression));
		}

		private bool Equals(RowNumberClauseOrderingsExpression? expression)
		{
			return base.Equals(expression) && Orderings.SequenceEqual(expression.Orderings);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			var hash = new HashCode();
			hash.Add(base.GetHashCode());

			for (var i = 0; i < Orderings.Count; i++)
			{
				hash.Add(Orderings[i]);
			}

			return hash.ToHashCode();
		}
	}


	/// <summary>
	/// Extension methods for <see cref="ExpressionVisitor"/>.
	/// </summary>
	public static class RelationalExpressionVisitorExtensions
	{
		/// <summary>
		/// Visits a collection of <paramref name="expressions"/> and returns new collection if it least one expression has been changed.
		/// Otherwise the provided <paramref name="expressions"/> are returned if there are no changes.
		/// </summary>
		/// <param name="visitor">Visitor to use.</param>
		/// <param name="expressions">Expressions to visit.</param>
		/// <returns>
		/// New collection with visited expressions if at least one visited expression has been changed; otherwise the provided <paramref name="expressions"/>.
		/// </returns>
		public static IReadOnlyList<T> VisitExpressions<T>(this ExpressionVisitor visitor, IReadOnlyList<T> expressions)
		   where T : Expression
		{
			if (visitor == null)
				throw new ArgumentNullException(nameof(visitor));
			if (expressions == null)
				throw new ArgumentNullException(nameof(expressions));

			var visitedExpressions = new List<T>();
			var hasChanges = false;

			foreach (var expression in expressions)
			{
				var visitedExpression = (T)visitor.Visit(expression);
				visitedExpressions.Add(visitedExpression);
				hasChanges |= !ReferenceEquals(visitedExpression, expression);
			}

			return hasChanges ? visitedExpressions.AsReadOnly() : expressions;
		}
	}


	/// <summary>
	/// Extension methods for <see cref="DbFunctions"/>.
	/// </summary>
	public static class RelationalDbFunctionsExtensions
	{
#pragma warning disable IDE0060 // Remove unused parameter

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <param name="_">An instance of <see cref="DbFunctions"/>.</param>
		/// <param name="orderBy">A column or an object containing columns to order by.</param>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber(this DbFunctions _, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1>(this DbFunctions _, T1 partitionByColumn1, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, T9 partitionByColumn9, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, T9 partitionByColumn9, T10 partitionByColumn10, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, T9 partitionByColumn9, T10 partitionByColumn10, T11 partitionByColumn11, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, T9 partitionByColumn9, T10 partitionByColumn10, T11 partitionByColumn11, T12 partitionByColumn12, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, T9 partitionByColumn9, T10 partitionByColumn10, T11 partitionByColumn11, T12 partitionByColumn12, T13 partitionByColumn13, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, T9 partitionByColumn9, T10 partitionByColumn10, T11 partitionByColumn11, T12 partitionByColumn12, T13 partitionByColumn13, T14 partitionByColumn14, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, T9 partitionByColumn9, T10 partitionByColumn10, T11 partitionByColumn11, T12 partitionByColumn12, T13 partitionByColumn13, T14 partitionByColumn14, T15 partitionByColumn15, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ROW_NUMBER.
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// </summary>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static long RowNumber<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this DbFunctions _, T1 partitionByColumn1, T2 partitionByColumn2, T3 partitionByColumn3, T4 partitionByColumn4, T5 partitionByColumn5, T6 partitionByColumn6, T7 partitionByColumn7, T8 partitionByColumn8, T9 partitionByColumn9, T10 partitionByColumn10, T11 partitionByColumn11, T12 partitionByColumn12, T13 partitionByColumn13, T14 partitionByColumn14, T15 partitionByColumn15, T16 partitionByColumn16, RowNumberOrderByClause orderBy)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ORDER BY clause of a the ROW_NUMBER expression.
		/// </summary>
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static RowNumberOrderByClause OrderBy<T>(this DbFunctions _, T column)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ORDER BY clause of a the ROW_NUMBER expression.
		/// </summary>
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static RowNumberOrderByClause OrderByDescending<T>(this DbFunctions _, T column)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ORDER BY clause of a the ROW_NUMBER expression.
		/// </summary>
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static RowNumberOrderByClause ThenBy<T>(this RowNumberOrderByClause clause, T column)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}

		/// <summary>
		/// Definition of the ORDER BY clause of a the ROW_NUMBER expression.
		/// </summary>
		/// <remarks>
		/// This method is for use with Entity Framework Core only and has no in-memory implementation.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
		public static RowNumberOrderByClause ThenByDescending<T>(this RowNumberOrderByClause clause, T column)
		{
			throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
		}
#pragma warning restore IDE0060 // Remove unused parameter

	}
}
