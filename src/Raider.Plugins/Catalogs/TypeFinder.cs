using Raider.Reflection.Loader;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Raider.Plugins.Catalogs
{
	public class TypeFinder
	{
		public static List<Type> Find(TypeFinderCriteria criteria, Assembly assembly, ITypeFindingContext typeFindingContext)
		{
			if (criteria == null)
				throw new ArgumentNullException(nameof(criteria));

			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			if (typeFindingContext == null)
				throw new ArgumentNullException(nameof(typeFindingContext));

			var result = new List<Type>();

			var types = assembly.GetExportedTypes();

			foreach (var type in types)
			{
				var isMatch = IsMatch(criteria, type, typeFindingContext);

				if (isMatch == false)
					continue;

				result.Add(type);
			}

			return result;
		}

		public static bool IsMatch(TypeFinderCriteria criteria, Type type, ITypeFindingContext typeFindingContext)
		{
			if (criteria == null)
				throw new ArgumentNullException(nameof(criteria));

			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (typeFindingContext == null)
				throw new ArgumentNullException(nameof(typeFindingContext));

			if (criteria.Query != null)
			{
				var isMatch = criteria.Query(typeFindingContext, type);
				return isMatch;
			}

			if (criteria.IsAbstract.HasValue && type.IsAbstract != criteria.IsAbstract.Value)
				return false;

			if (criteria.IsInterface.HasValue && type.IsInterface != criteria.IsInterface.Value)
				return false;

			if (!string.IsNullOrWhiteSpace(criteria.Name) && !string.IsNullOrWhiteSpace(type.FullName))
			{
				var regEx = NameToRegex(criteria.Name);

				if (!regEx.IsMatch(type.FullName))
				{
					var hasDirectNamingMatch = 
						string.Equals(criteria.Name, type.Name, StringComparison.InvariantCultureIgnoreCase)
						|| string.Equals(criteria.Name, type.FullName, StringComparison.InvariantCultureIgnoreCase);

					if (!hasDirectNamingMatch)
						return false;
				}
			}

			if (criteria.Inherits != null)
			{
				var inheritedType = typeFindingContext.FindType(criteria.Inherits);

				if (inheritedType == null || !inheritedType.IsAssignableFrom(type))
					return false;
			}

			if (criteria.Implements != null)
			{
				var interfaceType = typeFindingContext.FindType(criteria.Implements);

				if (interfaceType == null || !interfaceType.IsAssignableFrom(type))
					return false;
			}

			if (criteria.AssignableTo != null)
			{
				var assignableToType = typeFindingContext.FindType(criteria.AssignableTo);

				if (assignableToType == null || !assignableToType.IsAssignableFrom(type))
					return false;
			}

			if (criteria.HasAttribute != null)
			{
				var attributes = type.GetCustomAttributesData();
				var attributeFound = false;

				foreach (var attributeData in attributes)
				{
					if (!string.Equals(attributeData.AttributeType.FullName, criteria.HasAttribute.FullName, StringComparison.InvariantCultureIgnoreCase))
						continue;

					attributeFound = true;
					break;
				}

				if (attributeFound == false)
					return false;
			}

			return true;
		}

		public static bool IsMatch(TypeFinderCriteria criteria, Type type)
		{
			if (criteria == null)
				throw new ArgumentNullException(nameof(criteria));

			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (criteria.Query != null)
				return false;

			if (criteria.IsAbstract.HasValue && type.IsAbstract != criteria.IsAbstract.Value)
				return false;

			if (criteria.IsInterface.HasValue && type.IsInterface != criteria.IsInterface.Value)
				return false;

			if (!string.IsNullOrWhiteSpace(criteria.Name) && !string.IsNullOrWhiteSpace(type.FullName))
			{
				var regEx = NameToRegex(criteria.Name);

				if (!regEx.IsMatch(type.FullName))
				{
					var hasDirectNamingMatch =
						string.Equals(criteria.Name, type.Name, StringComparison.InvariantCultureIgnoreCase)
						|| string.Equals(criteria.Name, type.FullName, StringComparison.InvariantCultureIgnoreCase);

					if (!hasDirectNamingMatch)
						return false;
				}
			}

			if (criteria.Inherits != null)
			{
				if (!criteria.Inherits.IsAssignableFrom(type))
					return false;
			}

			if (criteria.Implements != null)
			{
				if (!criteria.Implements.IsAssignableFrom(type))
					return false;
			}

			if (criteria.AssignableTo != null)
			{
				if (!criteria.AssignableTo.IsAssignableFrom(type))
					return false;
			}

			if (criteria.HasAttribute != null)
			{
				var attributes = type.GetCustomAttributesData();
				var attributeFound = false;

				foreach (var attributeData in attributes)
				{
					if (!string.Equals(attributeData.AttributeType.FullName, criteria.HasAttribute.FullName, StringComparison.InvariantCultureIgnoreCase))
						continue;

					attributeFound = true;
					break;
				}

				if (attributeFound == false)
					return false;
			}

			return true;
		}

		private static Regex NameToRegex(string nameFilter)
		{
			// https://stackoverflow.com/a/30300521/66988
			var regex = "^" + Regex.Escape(nameFilter).Replace("\\?", ".").Replace("\\*", ".*") + "$";

			return new Regex(regex, RegexOptions.Compiled);
		}
	}
}
