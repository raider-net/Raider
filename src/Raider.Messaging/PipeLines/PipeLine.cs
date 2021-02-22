using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public class PipeLine<T> : IPipeLine<T?>
	{
		private readonly List<IAspect<T?>> _aspects = new List<IAspect<T?>>();

		public PipeLine()
		{
			_aspects.Add(new RootAspect());
		}

		public void AddAspect(IAspect<T?> aspect)
		{
			if (aspect == null)
				throw new ArgumentNullException(nameof(aspect));

			_aspects.Insert(_aspects.Count - 1, aspect);
			_aspects.Aggregate((a, b) => a.SetNext(b));
		}

		public void Invoke(T? data)
		{
			if (_aspects.Count == 0)
				return;

			var firstAspect = _aspects[0];
			firstAspect.Execute?.Invoke(data);
		}

		public Task InvokeAsync(T? data, CancellationToken cancellationToken = default)
		{
			if (_aspects.Count == 0)
				return Task.CompletedTask;

			var firstAspect = _aspects[0];
			return firstAspect.ExecuteAsync?.Invoke(data, cancellationToken) ?? Task.CompletedTask;
		}

		private class RootAspect : Aspect<T>
		{
			public override Action<T?>? Execute
				=> data => Next?.Execute?.Invoke(data);

			public override Func<T?, CancellationToken, Task>? ExecuteAsync
				=> (data, cancellationToken) => Next?.ExecuteAsync?.Invoke(data, cancellationToken) ?? Task.CompletedTask;
		}
	}
}
