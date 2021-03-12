using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Raider.Text
{
	public class TemplateFormatter
	{
		private readonly Dictionary<string, object?> _placeholderValues = new Dictionary<string, object?>();
		private static readonly Regex _keyRegex = new Regex("{([^{}:]+)(?::([^{}]+))?}", RegexOptions.Compiled);

		public Dictionary<string, object?> PlaceholderValues => _placeholderValues;

		public TemplateFormatter()
		{
		}

		public TemplateFormatter SetValue(string name, object value)
		{
			_placeholderValues[name] = value;
			return this;
		}

		public virtual string? Format(string template)
			=> ReplacePlaceholders(template, _placeholderValues);

		public virtual string? Format(string template, IDictionary<string, object?>? values)
			=> ReplacePlaceholders(template, values);

		private static string? ReplacePlaceholders(string template, IDictionary<string, object?>? values)
		{
			if (template == null)
				return null;

			if (values == null)
				return template;

			return _keyRegex.Replace(template, match => {
				var key = match.Groups[1].Value;

				if (!values.TryGetValue(key, out object? value))
					return match.Value;

				var format = match.Groups[2].Success
					? $"{{0:{match.Groups[2].Value}}}"
					: null;

				return format == null
					? (value?.ToString() ?? "")
					: string.Format(format, value);
			});
		}
	}
}
