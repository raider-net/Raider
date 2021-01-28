using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Raider.Localization
{
	public class Resource
	{
		public string Name { get; }
		public string Value { get; }
		public List<string> Parameters { get; }
		public List<string> Errors { get; }

		public Resource(ResxData data)
			: this(data?.Name ?? throw new ArgumentNullException(nameof(data)), data.Value)
		{
		}

		public Resource(string name, string value)
		{
			Name = name;
			Value = value;
			Value = Value?.Replace("\\", "\\\\");
			Value = Value?.Replace("\"", "\\\"");

			if (string.IsNullOrEmpty(Name))
			{
				Parameters = new List<string>();
				Errors = new List<string> { $"{nameof(Name)} is null" };
			}
			else if (!string.IsNullOrWhiteSpace(Value))
			{
				List<string> parameters = Regex.Matches(Value, @"\{(\w+)\}")
					.Cast<Match>()
					.Select(m => m.Groups[1].Value)
					.Distinct()
					.OrderBy(m => m)
					.ToList();

				Parameters = new List<string>();
				Errors = new List<string>();
				for (int i = 0; i < parameters.Count(); i++)
				{
					bool hasError = false;
					if (!int.TryParse(parameters[i], out int intValue))
					{
						Errors.Add($"{Name} has invalid formatting parameter {parameters[i]}. Can not cast to int.");
						hasError = true;
					}

					if (parameters.All(p => p != i.ToString()))
					{
						Errors.Add($"IndexOutOfRangeException: {Name} has invalid formatting parameters. Index out of range.");
						hasError = true;
					}

					if (!hasError)
					{
						Parameters.Add(parameters[i]);
					}
				}
			}
		}

		public Resource(DictionaryEntry? de)
			: this(de?.Key?.ToString(), de?.Value?.ToString())
		{
		}

		public override string ToString()
		{
			return $"{Name} = {Value}";
		}
	}

}
