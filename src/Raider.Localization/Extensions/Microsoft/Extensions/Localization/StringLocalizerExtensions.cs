using Microsoft.Extensions.Localization;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Extensions
{
	public static class StringLocalizerExtensions
	{
		[return: NotNullIfNotNull("defaultText")]
		public static string? GetLocalizedString(this IStringLocalizer localizer, string resourceKey)
			=> GetLocalizedString(localizer, resourceKey, resourceKey);

		[return: NotNullIfNotNull("defaultText")]
		public static string? GetLocalizedString(this IStringLocalizer localizer, string resourceKey, string? defaultText)
		{
			if (localizer == null)
			{
				return defaultText;
			}
			else
			{
				var localizedString = localizer[resourceKey];
				if (localizedString.ResourceNotFound)
				{
					return defaultText;
				}
				else
				{
					return localizedString;
				}
			}
		}

		[return: NotNullIfNotNull("defaultText")]
		public static string? GetLocalizedString(this IStringLocalizer localizer, string resourceKey, string? defaultText, params object[] arguments)
		{
			if (localizer == null)
			{
				return defaultText;
			}
			else
			{
				var localizedString = localizer[resourceKey, arguments];
				if (localizedString.ResourceNotFound)
				{
					return defaultText;
				}
				else
				{
					return localizedString;
				}
			}
		}

		public static bool TryGetLocalizedString(this IStringLocalizer localizer, string resourceKey, [NotNullWhen(true)] out string? localizedValue)
		{
			localizedValue = null;
			if (localizer == null)
			{
				return false;
			}
			else
			{
				var localizedString = localizer[resourceKey];
				if (localizedString.ResourceNotFound)
				{
					return false;
				}
				else
				{
					if (localizedString == null)
					{
						return false;
					}
					else
					{
						localizedValue = localizedString;
						return true;
					}
				}
			}
		}

		public static bool TryGetLocalizedString(this IStringLocalizer localizer, string resourceKey, [NotNullWhen(true)] out string? localizedValue, params object[] arguments)
		{
			localizedValue = null;
			if (localizer == null)
			{
				return false;
			}
			else
			{
				var localizedString = localizer[resourceKey, arguments];
				if (localizedString.ResourceNotFound)
				{
					return false;
				}
				else
				{
					if (localizedString == null)
					{
						return false;
					}
					else
					{
						localizedValue = localizedString;
						return true;
					}
				}
			}
		}
	}
}
