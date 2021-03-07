using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Raider.EntityFrameworkCore.Mapping
{
	public abstract partial class Mapper<TSource, TTarget, IFrom, ITo>
		where IFrom : notnull
	{
		[return: NotNullIfNotNull("source")]
		public Task<TTarget?> MapAsync(
			TSource? source,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TTarget?, Task>? postMapActionAsync = null)
			=> MapInternalAsync(source, default, new Dictionary<IFrom, ITo>(), referenceModifier, conditions, postMapActionAsync);

		[return: NotNullIfNotNull("source")]
		public Task<TTarget?> MapAsync(
			TSource? source,
			TTarget? target,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TTarget?, Task>? postMapActionAsync = null)
			=> MapInternalAsync(source, target, new Dictionary<IFrom, ITo>(), referenceModifier, conditions, postMapActionAsync);

		[return: NotNullIfNotNull("source")]
		public Task<List<TTarget>?> MapAsync(
			IEnumerable<TSource>? source,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TTarget?>, Task>? postMapActionAsync = null)
			=> MapToListAsync(source, (List<TTarget>?)null, new Dictionary<IFrom, ITo>(), MapInternalAsync, referenceModifier, conditions, postMapActionAsync);

		[return: NotNullIfNotNull("source")]
		public Task<ICollection<TTarget>?> MapAsync(
			IEnumerable<TSource>? source,
			ICollection<TTarget>? target,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TTarget?>, Task>? postMapActionAsync = null)
			=> MapToListAsync(source, target, new Dictionary<IFrom, ITo>(), MapInternalAsync, referenceModifier, conditions, postMapActionAsync);

		[return: NotNullIfNotNull("source")]
		public Task<TTarget?> MapAsync(
			TSource? source,
			out Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TTarget?, Task>? postMapActionAsync = null)
		{
			dict = new Dictionary<IFrom, ITo>();
			return MapInternalAsync(source, default, dict, referenceModifier, conditions, postMapActionAsync);
		}

		[return: NotNullIfNotNull("source")]
		public Task<TTarget?> MapAsync(
			TSource? source,
			TTarget? target,
			out Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TTarget?, Task>? postMapActionAsync = null)
		{
			dict = new Dictionary<IFrom, ITo>();
			return MapInternalAsync(source, target, dict, referenceModifier, conditions, postMapActionAsync);
		}

		[return: NotNullIfNotNull("source")]
		public Task<List<TTarget>?> MapAsync(
			IEnumerable<TSource>? source,
			out Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TTarget?>, Task>? postMapActionAsync = null)
		{
			dict = new Dictionary<IFrom, ITo>();
			return MapToListAsync(source, (List<TTarget>?)null, dict, MapInternalAsync, referenceModifier, conditions, postMapActionAsync);
		}

		[return: NotNullIfNotNull("source")]
		public Task<ICollection<TTarget>?> MapAsync(
			IEnumerable<TSource>? source,
			ICollection<TTarget>? target,
			out Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TTarget?>, Task>? postMapActionAsync = null)
		{
			dict = new Dictionary<IFrom, ITo>();
			return MapToListAsync(source, target, dict, MapInternalAsync, referenceModifier, conditions, postMapActionAsync);
		}

		[return: NotNullIfNotNull("source")]
		public static async Task<ICollection<TTarget>?> MapToListAsync(
			IEnumerable<TSource>? source,
			ICollection<TTarget>? target,
			Dictionary<IFrom, ITo> dict,
			Func<TSource, TTarget?, Dictionary<IFrom, ITo>, ReferenceModifier, Action<MappingConditions<TSource>>?, Func<TSource?, TTarget?, Task>?, Task<TTarget?>> map,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TTarget?>, Task>? postMapActionAsync = null)
		{
			if (map == null)
				throw new ArgumentNullException(nameof(map));

			if (target == null)
				return await MapToListAsync(source, (List<TTarget>?)target, dict, map, referenceModifier, conditions, postMapActionAsync);
			else if (target is List<TTarget> targetList)
				return await MapToListAsync(source, targetList, dict, map, referenceModifier, conditions, postMapActionAsync);
			else if (target is HashSet<TTarget> targetHashSet)
				return await MapToListAsync(source, targetHashSet, dict, map, referenceModifier, conditions, postMapActionAsync);
			else
				throw new NotSupportedException($"target type is {target.GetType().FullName}");
		}

		[return: NotNullIfNotNull("source")]
		private static async Task<List<TTarget>?> MapToListAsync(
			IEnumerable<TSource>? source,
			List<TTarget>? target,
			Dictionary<IFrom, ITo> dict,
			Func<TSource, TTarget?, Dictionary<IFrom, ITo>, ReferenceModifier, Action<MappingConditions<TSource>>?, Func<TSource?, TTarget?, Task>?, Task<TTarget?>> map,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TTarget?>, Task>? postMapActionAsync = null)
		{
			if (source == null)
			{
				target?.Clear();
				return null;
			}

			var targetCount = target?.Count ?? 0;

			if (target == null)
				target = new List<TTarget>();

			var tar = target.GetEnumerator();
			var targetIndex = 0;
			var mappedCount = 0;
			using (var src = source.GetEnumerator())
			{
				while (src.MoveNext())
				{
					mappedCount++;
					if (targetIndex < targetCount && tar.MoveNext())
					{
						await map(src.Current, tar.Current, dict, referenceModifier, conditions, null);
					}
					else
					{
						var itemResult = await map(src.Current, default, dict, referenceModifier, conditions, null);
						if (itemResult != null)
							target.Add(itemResult);
					}
					targetIndex++;
				}
			}

			if (mappedCount == 0)
			{
				target.Clear();
			}
			else if (targetIndex < targetCount)
			{
				TTarget[] tmpTargets = new TTarget[targetCount - targetIndex];
				targetIndex = 0;
				using (tar)
				{
					while (tar.MoveNext())
						tmpTargets[targetIndex++] = tar.Current;
				}

				foreach (var tmp in tmpTargets)
					target.Remove(tmp);
			}

			if (postMapActionAsync != null)
				await postMapActionAsync.Invoke(source, target);

			return target;
		}

		[return: NotNullIfNotNull("source")]
		private static async Task<HashSet<TTarget>?> MapToListAsync(
			IEnumerable<TSource>? source,
			HashSet<TTarget>? target,
			Dictionary<IFrom, ITo> dict,
			Func<TSource, TTarget?, Dictionary<IFrom, ITo>, ReferenceModifier, Action<MappingConditions<TSource>>?, Func<TSource?, TTarget?, Task>?, Task<TTarget?>> map,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TTarget?>, Task>? postMapActionAsync = null)
		{
			if (source == null)
			{
				target?.Clear();
				return null;
			}

			var targetCount = target?.Count ?? 0;

			if (target == null)
				target = new HashSet<TTarget>();

			var tar = target.GetEnumerator();
			var targetIndex = 0;
			var mappedCount = 0;
			using (var src = source.GetEnumerator())
			{
				while (src.MoveNext())
				{
					mappedCount++;
					if (targetIndex < targetCount && tar.MoveNext())
					{
						await map(src.Current, tar.Current, dict, referenceModifier, conditions, null);
					}
					else
					{
						var itemResult = await map(src.Current, default, dict, referenceModifier, conditions, null);
						if (itemResult != null)
							target.Add(itemResult);
					}
					targetIndex++;
				}
			}

			if (mappedCount == 0)
			{
				target.Clear();
			}
			else if (targetIndex < targetCount)
			{
				TTarget[] tmpTargets = new TTarget[targetCount - targetIndex];
				targetIndex = 0;
				using (tar)
				{
					while (tar.MoveNext())
						tmpTargets[targetIndex++] = tar.Current;
				}

				foreach (var tmp in tmpTargets)
					target.Remove(tmp);
			}

			if (postMapActionAsync != null)
				await postMapActionAsync.Invoke(source, target);

			return target;
		}

		[return: NotNullIfNotNull("source")]
		protected virtual Task<TTarget?> MapInternalAsync(
			TSource? source,
			TTarget? target,
			Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TTarget?, Task>? postMapActionAsync = null)
			=> ApplyPostActionAsync(source, target, postMapActionAsync);

		protected virtual async Task<TTarget?> ApplyPostActionAsync(TSource? source, TTarget? target, Func<TSource?, TTarget?, Task>? postMapActionAsync)
		{
			if (target != null && postMapActionAsync != null)
				await postMapActionAsync.Invoke(source, target);

			return target;
		}

		[return: NotNullIfNotNull("source")]
		public Task<TSource?> CopyAsync(
			TSource? source,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TSource?, Task>? postMapActionAsync = null)
			=> CopyInternalAsync(source, default, new Dictionary<IFrom, IFrom>(), referenceModifier, conditions, postMapActionAsync);

		[return: NotNullIfNotNull("source")]
		public Task<TSource?> CopyAsync(
			TSource? source,
			TSource? target,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TSource?, Task>? postMapActionAsync = null)
			=> CopyInternalAsync(source, target, new Dictionary<IFrom, IFrom>(), referenceModifier, conditions, postMapActionAsync);

		[return: NotNullIfNotNull("source")]
		public Task<List<TSource>?> CopyAsync(
			IEnumerable<TSource>? source,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TSource?>, Task>? postMapActionAsync = null)
			=> CopyListAsync(source, (List<TSource>?)null, new Dictionary<IFrom, IFrom>(), CopyInternalAsync, referenceModifier, conditions, postMapActionAsync);

		[return: NotNullIfNotNull("source")]
		public Task<ICollection<TSource>?> CopyAsync(
			IEnumerable<TSource>? source,
			ICollection<TSource>? target,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TSource?>, Task>? postMapActionAsync = null)
			=> CopyListAsync(source, target, new Dictionary<IFrom, IFrom>(), CopyInternalAsync, referenceModifier, conditions, postMapActionAsync);

		[return: NotNullIfNotNull("source")]
		public Task<TSource?> CopyAsync(
			TSource? source,
			out Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TSource?, Task>? postMapActionAsync = null)
		{
			dict = new Dictionary<IFrom, IFrom>();
			return CopyInternalAsync(source, default, dict, referenceModifier, conditions, postMapActionAsync);
		}

		[return: NotNullIfNotNull("source")]
		public Task<TSource?> CopyAsync(
			TSource? source,
			TSource? target,
			out Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TSource?, Task>? postMapActionAsync = null)
		{
			dict = new Dictionary<IFrom, IFrom>();
			return CopyInternalAsync(source, target, dict, referenceModifier, conditions, postMapActionAsync);
		}

		[return: NotNullIfNotNull("source")]
		public Task<List<TSource>?> CopyAsync(
			IEnumerable<TSource>? source,
			out Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TSource?>, Task>? postMapActionAsync = null)
		{
			dict = new Dictionary<IFrom, IFrom>();
			return CopyListAsync(source, (List<TSource>?)null, dict, CopyInternalAsync, referenceModifier, conditions, postMapActionAsync);
		}

		[return: NotNullIfNotNull("source")]
		public Task<ICollection<TSource>?> CopyAsync(
			IEnumerable<TSource>? source,
			ICollection<TSource>? target,
			out Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TSource?>, Task>? postMapActionAsync = null)
		{
			dict = new Dictionary<IFrom, IFrom>();
			return CopyListAsync(source, target, dict, CopyInternalAsync, referenceModifier, conditions, postMapActionAsync);
		}

		[return: NotNullIfNotNull("source")]
		public static async Task<ICollection<TSource>?> CopyListAsync(
			IEnumerable<TSource>? source,
			ICollection<TSource>? target,
			Dictionary<IFrom, IFrom> dict,
			Func<TSource, TSource?, Dictionary<IFrom, IFrom>, ReferenceModifier, Action<MappingConditions<TSource>>?, Func<TSource?, TSource?, Task>?, Task<TSource?>> copy,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TSource?>, Task>? postMapActionAsync = null)
		{
			if (copy == null)
				throw new ArgumentNullException(nameof(copy));

			if (target == null)
				return await CopyListAsync(source, (List<TSource>?)target, dict, copy, referenceModifier, conditions, postMapActionAsync);
			else if (target is List<TSource> targetList)
				return await CopyListAsync(source, targetList, dict, copy, referenceModifier, conditions, postMapActionAsync);
			else if (target is HashSet<TSource> targetHashSet)
				return await CopyListAsync(source, targetHashSet, dict, copy, referenceModifier, conditions, postMapActionAsync);
			else
				throw new NotSupportedException($"target type is {target.GetType().FullName}");
		}

		[return: NotNullIfNotNull("source")]
		private static async Task<List<TSource>?> CopyListAsync(
			IEnumerable<TSource>? source,
			List<TSource>? target,
			Dictionary<IFrom, IFrom> dict,
			Func<TSource, TSource?, Dictionary<IFrom, IFrom>, ReferenceModifier, Action<MappingConditions<TSource>>?, Func<TSource?, TSource?, Task>?, Task<TSource?>> copy,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TSource?>, Task>? postMapActionAsync = null)
		{
			if (source == null)
			{
				target?.Clear();
				return null;
			}

			var targetCount = target?.Count ?? 0;

			if (target == null)
				target = new List<TSource>();

			var tar = target.GetEnumerator();
			var targetIndex = 0;
			var mappedCount = 0;
			using (var src = source.GetEnumerator())
			{
				while (src.MoveNext())
				{
					mappedCount++;
					if (targetIndex < targetCount && tar.MoveNext())
					{
						await copy(src.Current, tar.Current, dict, referenceModifier, conditions, null);
					}
					else
					{
						var itemResult = await copy(src.Current, default, dict, referenceModifier, conditions, null);
						if (itemResult != null)
							target.Add(itemResult);
					}
					targetIndex++;
				}
			}

			if (mappedCount == 0)
			{
				target.Clear();
			}
			else if (targetIndex < targetCount)
			{
				TSource[] tmpTargets = new TSource[targetCount - targetIndex];
				targetIndex = 0;
				using (tar)
				{
					while (tar.MoveNext())
						tmpTargets[targetIndex++] = tar.Current;
				}

				foreach (var tmp in tmpTargets)
					target.Remove(tmp);
			}

			if (postMapActionAsync != null)
				await postMapActionAsync.Invoke(source, target);

			return target;
		}

		[return: NotNullIfNotNull("source")]
		private static async Task<HashSet<TSource>?> CopyListAsync(
			IEnumerable<TSource>? source,
			HashSet<TSource>? target,
			Dictionary<IFrom, IFrom> dict,
			Func<TSource, TSource?, Dictionary<IFrom, IFrom>, ReferenceModifier, Action<MappingConditions<TSource>>?, Func<TSource?, TSource?, Task>?, Task<TSource?>> copy,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<IEnumerable<TSource?>, IEnumerable<TSource?>, Task>? postMapActionAsync = null)
		{
			if (source == null)
			{
				target?.Clear();
				return null;
			}

			var targetCount = target?.Count ?? 0;

			if (target == null)
				target = new HashSet<TSource>();

			var tar = target.GetEnumerator();
			var targetIndex = 0;
			var mappedCount = 0;
			using (var src = source.GetEnumerator())
			{
				while (src.MoveNext())
				{
					mappedCount++;
					if (targetIndex < targetCount && tar.MoveNext())
					{
						await copy(src.Current, tar.Current, dict, referenceModifier, conditions, null);
					}
					else
					{
						var itemResult = await copy(src.Current, default, dict, referenceModifier, conditions, null);
						if (itemResult != null)
							target.Add(itemResult);
					}
					targetIndex++;
				}
			}

			if (mappedCount == 0)
			{
				target.Clear();
			}
			else if (targetIndex < targetCount)
			{
				TSource[] tmpTargets = new TSource[targetCount - targetIndex];
				targetIndex = 0;
				using (tar)
				{
					while (tar.MoveNext())
						tmpTargets[targetIndex++] = tar.Current;
				}

				foreach (var tmp in tmpTargets)
					target.Remove(tmp);
			}

			if (postMapActionAsync != null)
				await postMapActionAsync.Invoke(source, target);

			return target;
		}

		[return: NotNullIfNotNull("source")]
		protected virtual Task<TSource?> CopyInternalAsync(
			TSource? source,
			TSource? target,
			Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Func<TSource?, TSource?, Task>? postMapActionAsync = null)
			=> ApplyPostActionAsync(source, target, postMapActionAsync);

		protected virtual async Task<TSource?> ApplyPostActionAsync(TSource? source, TSource? target, Func<TSource?, TSource?, Task>? postMapActionAsync)
		{
			if (target != null && postMapActionAsync != null)
				await postMapActionAsync.Invoke(source, target);

			return target;
		}
	}
}
