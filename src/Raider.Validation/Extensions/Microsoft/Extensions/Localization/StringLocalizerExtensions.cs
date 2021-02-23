using Microsoft.Extensions.Localization;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Extensions
{
	public static class StringLocalizerExtensions
	{
		[return: NotNullIfNotNull("defaultText")]
		public static string? GetLocalizedString(this IStringLocalizer localizer, string resourceKey, string? defaultText = null)
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
		public static string? GetLocalizedString(this IStringLocalizer localizer, string resourceKey, string? defaultText = null, params object[] arguments)
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
	}
}
