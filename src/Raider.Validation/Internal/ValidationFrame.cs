#if NETSTANDARD2_0 || NETSTANDARD2_1
using Raider.Extensions;
#endif

using System;
using System.Collections.Generic;

namespace Raider.Validation
{
	internal abstract class ValidationFrame : IValidationFrame
	{
		public string? ObjectType { get; protected set; }
		public Dictionary<string, ValidationFrame>? Properties { get; protected set; }
		public string? PropertyName { get; protected set; }
		public ValidationFrame? Parent { get; protected set; }
		public int? Index { get; protected set; }
		public int Depth { get; protected set; }
		public string? PropertyNameWithIndex => GetPropertyNameWithIndex();

		IValidationFrame? IValidationFrame.Parent => Parent;

		public ValidationFrameProperty AddProperty(string objectType, string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException(nameof(propertyName));

			if (Properties == null)
				throw new NotSupportedException($"{GetType().FullName} has no properties.");

			var property = new ValidationFrameProperty(this, objectType, propertyName);
			Properties.TryAdd(propertyName, property);
			return property;
		}

		public ValidationFrameNavigation AddNavigation(string objectType, string propertyName)
		{
			if (string.IsNullOrWhiteSpace(objectType))
				throw new ArgumentNullException(nameof(objectType));

			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException(nameof(propertyName));

			if (Properties == null)
				throw new NotSupportedException($"{GetType().FullName} has no properties.");

			var navigation = new ValidationFrameNavigation(this, objectType, propertyName);
			Properties.TryAdd(propertyName, navigation);
			return navigation;
		}

		public ValidationFrameEnumeration AddEnumeration(string objectType, string propertyName)
		{
			if (string.IsNullOrWhiteSpace(objectType))
				throw new ArgumentNullException(nameof(objectType));

			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException(nameof(propertyName));

			if (Properties == null)
				throw new NotSupportedException($"{GetType().FullName} has no properties.");

			var enumeration = new ValidationFrameEnumeration(this, objectType, propertyName);
			Properties.TryAdd(propertyName, enumeration);
			return enumeration;
		}

		public ValidationFrame SetParent(ValidationFrame parent, bool parentCanBeNull)
		{
			if (parent == null)
				throw new ArgumentNullException(nameof(parent));

			if (!parentCanBeNull)
			{
				if (parentCanBeNull || Parent == null)
					throw new InvalidOperationException($"Parent is null.");

				if (Parent.ObjectType != parent.ObjectType)
					throw new InvalidOperationException($"Invalid {nameof(ObjectType)}.");
			}

			Parent = parent;

			return this;
		}

		public string? GetPropertyNameWithIndex()
		{
			if (Index.HasValue)
			{
				if (string.IsNullOrWhiteSpace(PropertyName))
					throw new InvalidOperationException($"PropertyName is null, but {nameof(Index)} = {Index}");

				return $"{PropertyName}[{Index}]";
			}
			return PropertyName;
		}

		public override string ToString()
		{
			var path = new List<string> { GetPropertyNameWithIndex() ?? "_" };
			var parent = Parent;
			while (parent != null)
			{
				path.Add(parent.PropertyNameWithIndex ?? "_");
				parent = parent.Parent;
			}

			path.Reverse();
			return string.Join(".", path);
		}
	}

	internal class ValidationFrameRoot : ValidationFrame
	{
		public ValidationFrameRoot(string objectType)
		{
			ObjectType = string.IsNullOrWhiteSpace(objectType) ? throw new ArgumentNullException(nameof(objectType)) : objectType;
			Properties = new Dictionary<string, ValidationFrame>();
			Parent = null;
			PropertyName = null;
			Index = null;
			Depth = 0;
		}
	}

	internal class ValidationFrameProperty : ValidationFrame
	{
		public ValidationFrameProperty(ValidationFrame parent, string objectType, string propertyName)
		{
			ObjectType = string.IsNullOrWhiteSpace(objectType) ? throw new ArgumentNullException(nameof(objectType)) : objectType;
			Properties = null;
			Parent = parent ?? throw new ArgumentNullException(nameof(parent));
			PropertyName = string.IsNullOrWhiteSpace(propertyName) ? throw new ArgumentNullException(nameof(parent)) : propertyName;
			Index = null;
			Depth = Parent.Depth + 1;
		}
	}

	internal class ValidationFrameNavigation : ValidationFrame
	{
		public ValidationFrameNavigation(ValidationFrame parent, string objectType, string propertyName)
		{
			ObjectType = string.IsNullOrWhiteSpace(objectType) ? throw new ArgumentNullException(nameof(objectType)) : objectType;
			Properties = new Dictionary<string, ValidationFrame>();
			Parent = parent ?? throw new ArgumentNullException(nameof(parent));
			PropertyName = string.IsNullOrWhiteSpace(propertyName) ? throw new ArgumentNullException(nameof(parent)) : propertyName;
			Index = null;
			Depth = Parent.Depth + 1;
		}
	}

	internal class ValidationFrameEnumeration : ValidationFrameNavigation
	{
		public ValidationFrameEnumeration(ValidationFrame parent, string objectType, string propertyName)
			: base(parent, objectType, propertyName)
		{
		}
	}
}
