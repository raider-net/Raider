﻿using System;
using System.Collections.Generic;

namespace Raider.EntityFrameworkCore.Mapping
{
	public abstract class Mapper<TSource, TTarget, IFrom, ITo>
		where IFrom : notnull
	{
		public TTarget? Map(
			TSource? source,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
			=> MapInternal(source, default, new Dictionary<IFrom, ITo>(), referenceModifier, conditions, postMapAction);

		public TTarget? Map(
			TSource? source,
			TTarget? target,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
			=> MapInternal(source, target, new Dictionary<IFrom, ITo>(), referenceModifier, conditions, postMapAction);

		public List<TTarget>? Map(
			IEnumerable<TSource>? source,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
			=> MapToList(source, (List<TTarget>?)null, new Dictionary<IFrom, ITo>(), MapInternal, referenceModifier, conditions, postMapAction);

		public ICollection<TTarget>? Map(
			IEnumerable<TSource>? source,
			ICollection<TTarget>? target,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
			=> MapToList(source, target, new Dictionary<IFrom, ITo>(), MapInternal, referenceModifier, conditions, postMapAction);

		public TTarget? Map(
			TSource? source,
			out Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
		{
			dict = new Dictionary<IFrom, ITo>();
			return MapInternal(source, default, dict, referenceModifier, conditions, postMapAction);
		}

		public TTarget? Map(
			TSource? source,
			TTarget? target,
			out Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
		{
			dict = new Dictionary<IFrom, ITo>();
			return MapInternal(source, target, dict, referenceModifier, conditions, postMapAction);
		}

		public List<TTarget>? Map(
			IEnumerable<TSource>? source,
			out Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
		{
			dict = new Dictionary<IFrom, ITo>();
			return MapToList(source, (List<TTarget>?)null, dict, MapInternal, referenceModifier, conditions, postMapAction);
		}

		public ICollection<TTarget>? Map(
			IEnumerable<TSource>? source,
			ICollection<TTarget>? target,
			out Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
		{
			dict = new Dictionary<IFrom, ITo>();
			return MapToList(source, target, dict, MapInternal, referenceModifier, conditions, postMapAction);
		}

		public static ICollection<TTarget>? MapToList(
			IEnumerable<TSource>? source,
			ICollection<TTarget>? target,
			Dictionary<IFrom, ITo> dict,
			Func<TSource, TTarget?, Dictionary<IFrom, ITo>, ReferenceModifier, Action<MappingConditions<TSource>>?, Action<TTarget>?, TTarget?> map,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
		{
			if (map == null)
				throw new ArgumentNullException(nameof(map));

			if (target == null)
				return MapToList(source, (List<TTarget>?)target, dict, map, referenceModifier, conditions, postMapAction);
			else if (target is List<TTarget> targetList)
				return MapToList(source, targetList, dict, map, referenceModifier, conditions, postMapAction);
			else if (target is HashSet<TTarget> targetHashSet)
				return MapToList(source, targetHashSet, dict, map, referenceModifier, conditions, postMapAction);
			else
				throw new NotSupportedException($"target type is {target.GetType().FullName}");
		}

		private static List<TTarget>? MapToList(
			IEnumerable<TSource>? source,
			List<TTarget>? target,
			Dictionary<IFrom, ITo> dict,
			Func<TSource, TTarget?, Dictionary<IFrom, ITo>, ReferenceModifier, Action<MappingConditions<TSource>>?, Action<TTarget>?, TTarget?> map,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
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
						map(src.Current, tar.Current, dict, referenceModifier, conditions, postMapAction);
					}
					else
					{
						var itemResult = map(src.Current, default, dict, referenceModifier, conditions, postMapAction);
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

			return target;
		}

		private static HashSet<TTarget>? MapToList(
			IEnumerable<TSource>? source,
			HashSet<TTarget>? target,
			Dictionary<IFrom, ITo> dict,
			Func<TSource, TTarget?, Dictionary<IFrom, ITo>, ReferenceModifier, Action<MappingConditions<TSource>>?, Action<TTarget>?, TTarget?> map,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
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
						map(src.Current, tar.Current, dict, referenceModifier, conditions, postMapAction);
					}
					else
					{
						var itemResult = map(src.Current, default, dict, referenceModifier, conditions, postMapAction);
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

			return target;
		}

		protected virtual TTarget? MapInternal(
			TSource? source,
			TTarget? target,
			Dictionary<IFrom, ITo> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TTarget>? postMapAction = null)
			=> ApplyPostAction(target, postMapAction);

		protected virtual TTarget? ApplyPostAction(TTarget? target, Action<TTarget>? postMapAction)
		{
			if (target != null)
				postMapAction?.Invoke(target);

			return target;
		}









		public TSource? Copy(
			TSource? source,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
			=> CopyInternal(source, default, new Dictionary<IFrom, IFrom>(), referenceModifier, conditions, postMapAction);

		public TSource? Copy(
			TSource? source,
			TSource? target,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
			=> CopyInternal(source, target, new Dictionary<IFrom, IFrom>(), referenceModifier, conditions, postMapAction);

		public List<TSource>? Copy(
			IEnumerable<TSource>? source,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
			=> CopyList(source, (List<TSource>?)null, new Dictionary<IFrom, IFrom>(), CopyInternal, referenceModifier, conditions, postMapAction);

		public ICollection<TSource>? Copy(
			IEnumerable<TSource>? source,
			ICollection<TSource>? target,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
			=> CopyList(source, target, new Dictionary<IFrom, IFrom>(), CopyInternal, referenceModifier, conditions, postMapAction);

		public TSource? Copy(
			TSource? source,
			out Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
		{
			dict = new Dictionary<IFrom, IFrom>();
			return CopyInternal(source, default, dict, referenceModifier, conditions, postMapAction);
		}

		public TSource? Copy(
			TSource? source,
			TSource? target,
			out Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
		{
			dict = new Dictionary<IFrom, IFrom>();
			return CopyInternal(source, target, dict, referenceModifier, conditions, postMapAction);
		}

		public List<TSource>? Copy(
			IEnumerable<TSource>? source,
			out Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
		{
			dict = new Dictionary<IFrom, IFrom>();
			return CopyList(source, (List<TSource>?)null, dict, CopyInternal, referenceModifier, conditions, postMapAction);
		}

		public ICollection<TSource>? Copy(
			IEnumerable<TSource>? source,
			ICollection<TSource>? target,
			out Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
		{
			dict = new Dictionary<IFrom, IFrom>();
			return CopyList(source, target, dict, CopyInternal, referenceModifier, conditions, postMapAction);
		}

		public static ICollection<TSource>? CopyList(
			IEnumerable<TSource>? source,
			ICollection<TSource>? target,
			Dictionary<IFrom, IFrom> dict,
			Func<TSource, TSource?, Dictionary<IFrom, IFrom>, ReferenceModifier, Action<MappingConditions<TSource>>?, Action<TSource>?, TSource?> copy,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
		{
			if (copy == null)
				throw new ArgumentNullException(nameof(copy));

			if (target == null)
				return CopyList(source, (List<TSource>?)target, dict, copy, referenceModifier, conditions, postMapAction);
			else if (target is List<TSource> targetList)
				return CopyList(source, targetList, dict, copy, referenceModifier, conditions, postMapAction);
			else if (target is HashSet<TSource> targetHashSet)
				return CopyList(source, targetHashSet, dict, copy, referenceModifier, conditions, postMapAction);
			else
				throw new NotSupportedException($"target type is {target.GetType().FullName}");
		}

		private static List<TSource>? CopyList(
			IEnumerable<TSource>? source,
			List<TSource>? target,
			Dictionary<IFrom, IFrom> dict,
			Func<TSource, TSource?, Dictionary<IFrom, IFrom>, ReferenceModifier, Action<MappingConditions<TSource>>?, Action<TSource>?, TSource?> copy,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
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
						copy(src.Current, tar.Current, dict, referenceModifier, conditions, postMapAction);
					}
					else
					{
						var itemResult = copy(src.Current, default, dict, referenceModifier, conditions, postMapAction);
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

			return target;
		}

		private static HashSet<TSource>? CopyList(
			IEnumerable<TSource>? source,
			HashSet<TSource>? target,
			Dictionary<IFrom, IFrom> dict,
			Func<TSource, TSource?, Dictionary<IFrom, IFrom>, ReferenceModifier, Action<MappingConditions<TSource>>?, Action<TSource>?, TSource?> copy,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
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
						copy(src.Current, tar.Current, dict, referenceModifier, conditions, postMapAction);
					}
					else
					{
						var itemResult = copy(src.Current, default, dict, referenceModifier, conditions, postMapAction);
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

			return target;
		}

		protected virtual TSource? CopyInternal(
			TSource? source,
			TSource? target,
			Dictionary<IFrom, IFrom> dict,
			ReferenceModifier referenceModifier = ReferenceModifier.SkipAllReferences,
			Action<MappingConditions<TSource>>? conditions = null,
			Action<TSource>? postMapAction = null)
			=> ApplyPostAction(target, postMapAction);

		protected virtual TSource? ApplyPostAction(TSource? target, Action<TSource>? postMapAction)
		{
			if (target != null)
				postMapAction?.Invoke(target);

			return target;
		}
	}
}