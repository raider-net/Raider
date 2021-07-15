using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Policy.Internal
{
	internal class ExceptionPredicates
	{
		public static readonly ExceptionPredicates None = new ExceptionPredicates();

		private List<ExceptionPredicate>? _predicates;

		internal void Add(ExceptionPredicate predicate)
		{
			_predicates ??= new List<ExceptionPredicate>();
			_predicates.Add(predicate);
		}

		public Exception? FirstMatchOrDefault(Exception ex)
			=> _predicates?.Select(predicate => predicate(ex)).FirstOrDefault(e => e != null);
	}
}
