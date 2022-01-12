using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Raider.Text
{
	public class CharInfo
	{
		public char Char { get; private set; }
		public int Int => (int)Char;
		public string Hex { get; private set; }

		public CharInfo()
		{
		}

		public CharInfo(char character)
		{
			Char = character;
			Hex = Convert.ToInt32(character).ToString("x");
		}

		public CharInfo(int character)
			: this((char)character)
		{
		}

		public CharInfo(string hex)
		{
			Hex = hex;
			int value = Convert.ToInt32(hex, 16);
			//string stringValue = Char.ConvertFromUtf32(value);
			Char = (char)value;
		}

		public override string ToString()
		{
			return Char + " = 0x" + Hex;
		}
	}

	public static class StringHelper
	{
		public const string DIGITS = @"0123456789";
		public const string AVAILABLE_FIRST_CHARS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
		public const string AVAILABLE_CHARS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789";
		public const string AVAILABLE_CHARS_NoUnderscore = @"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		public static string ToCammelCase(string text, bool strictCammelCase = false, bool removeUnderscores = true, bool throwIfEmpty = true)
		{
			if (string.IsNullOrWhiteSpace(text)) return text;

			var digits = new List<char>(DIGITS.ToCharArray());
			var firstChars = new List<char>(AVAILABLE_FIRST_CHARS.ToCharArray());
			firstChars.AddRange(AVAILABLE_FIRST_CHARS.ToLower().ToCharArray());
			List<char> allChars;
			if (removeUnderscores)
			{
				allChars = new List<char>(AVAILABLE_CHARS_NoUnderscore.ToCharArray());
				allChars.AddRange(AVAILABLE_CHARS_NoUnderscore.ToLower().ToCharArray());
			}
			else
			{
				allChars = new List<char>(AVAILABLE_CHARS.ToCharArray());
				allChars.AddRange(AVAILABLE_CHARS_NoUnderscore.ToLower().ToCharArray());
			}
			var normalizedTextBuilder = new StringBuilder();
			bool isFirst = true;
			bool toUpper = false;
			text = RemoveAccents(text);
			foreach (char ch in text.ToCharArray())
			{
				if (isFirst)
				{
					if (firstChars.Contains(ch))
					{
						normalizedTextBuilder.Append(char.ToUpper(ch));
						isFirst = false;
					}
					else if (digits.Contains(ch))
					{
						normalizedTextBuilder.Append($"_{ch}");
						isFirst = false;
					}
				}
				else
				{
					if (allChars.Contains(ch))
					{
						if (toUpper)
						{
							normalizedTextBuilder.Append(char.ToUpper(ch));
							toUpper = false;
						}
						else
						{
							normalizedTextBuilder.Append(ch);
						}
					}
					else
					{
						toUpper = true;
					}
				}
			}

			string normalizedText = normalizedTextBuilder.ToString();

			if (string.IsNullOrWhiteSpace(normalizedText))
			{
				if (throwIfEmpty || removeUnderscores)
					throw new Exception("Text '" + text + "' cannot be normalized.");
				else
					normalizedText = "_";
			}

			string result;
			if (strictCammelCase)
			{
				var resultBuilder = new StringBuilder();
				bool previousWasUpper = false;
				foreach (var ch in normalizedText)
				{
					if (ch == '_')
						continue;

					if (char.IsUpper(ch))
					{
						if (previousWasUpper)
							resultBuilder.Append(char.ToLower(ch));
						else
							resultBuilder.Append(ch);

						previousWasUpper = true;
					}
					else
					{
						resultBuilder.Append(ch);
						previousWasUpper = false;
					}
				}
				result = resultBuilder.ToString();
			}
			else
			{
				result = normalizedText;
			}

			if (string.IsNullOrWhiteSpace(result))
				return "_";

			return result;
		}

		public static List<CharInfo> GetCharInfos(List<char> chars)
		{
			if (chars == null) return null;
			List<CharInfo> result = new List<CharInfo>();
			foreach (char ch in chars)
			{
				result.Add(new CharInfo(ch));
			}
			return result;
		}

		public static List<CharInfo> GetCharInfos(List<int> ints)
		{
			if (ints == null) return null;
			return GetCharInfos(ints.Select(x => (char)x).ToList());
		}

		public static List<CharInfo> GetCharInfos(string text)
		{
			if (text == null) return null;
			return GetCharInfos(text.ToCharArray().ToList());
		}

		public static List<CharInfo> GetCharInfos(List<string> hexCodes)
		{
			if (hexCodes == null) return null;
			List<CharInfo> result = new List<CharInfo>();
			foreach (string hexCode in hexCodes)
			{
				CharInfo chInfo = new CharInfo(hexCode);
				result.Add(chInfo);
			}
			return result;
		}

		public static string CharInfosToString(List<CharInfo> charInfos)
		{
			if (charInfos == null) return null;
			if (charInfos.Count == 0) return string.Empty;
			StringBuilder sb = new StringBuilder();
			foreach (CharInfo chInfo in charInfos)
			{
				sb.Append(chInfo.Char);
			}
			return sb.ToString();
		}

		public static string FormatSafe(string format, params object[] args)
		{
			if (format == null || args == null)
			{
				return format;
			}
			try
			{
				return string.Format(format, args);
			}
			catch
			{
				return format;
			}
		}

		public static string FormatSafe(string format, object arg0)
		{
			if (format == null || arg0 == null)
			{
				return format;
			}
			try
			{
				return string.Format(format, arg0);
			}
			catch
			{
				return format;
			}
		}

		public static string FormatSafe(IFormatProvider provider, string format, params object[] args)
		{
			if (format == null || args == null)
			{
				return format;
			}
			try
			{
				return string.Format(provider, format, args);
			}
			catch
			{
				return format;
			}
		}

		public static string FormatSafe(string format, object arg0, object arg1)
		{
			if (format == null || (arg0 == null && arg1 == null))
			{
				return format;
			}
			try
			{
				return string.Format(format, arg0, arg1);
			}
			catch
			{
				return format;
			}
		}

		public static string FormatSafe(string format, object arg0, object arg1, object arg2)
		{
			if (format == null || (arg0 == null && arg1 == null && arg2 == null))
			{
				return format;
			}
			try
			{
				return string.Format(format, arg0, arg1, arg2);
			}
			catch
			{
				return format;
			}
		}

		public static string Combine(string text1, string text2, string joiner = null)
		{
			if (text1 == null)
				return text2;

			if (string.IsNullOrEmpty(text1))
			{
				return text2 ?? text1;
			}

			if (string.IsNullOrEmpty(text2))
			{
				return text1;
			}

			return joiner == null
				? $"{text1}{text2}"
				: $"{text1}{joiner}{text2}";
		}

		public static string TrimPrefix(string text, string prefix, bool ignoreCase = false)
		{
			if (text == null)
				return null;

			if (string.IsNullOrEmpty(prefix))
				return text;

			if (text.StartsWith(prefix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return text.Substring(prefix.Length);
			}

			return text;
		}

		public static string TrimPostfix(string text, string postfix, bool ignoreCase = false)
		{
			if (text == null)
				return null;

			if (string.IsNullOrEmpty(postfix))
				return text;

			if (text.EndsWith(postfix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return text.Substring(0, text.Length - postfix.Length);
			}

			return text;
		}

		public static string? TrimLength(string text, int maxLength, string? postfix = null)
		{
			if (text == null)
				return null;

			if (maxLength <= 0)
				return "";

			if (text.Length <= maxLength)
				return text;

			if (string.IsNullOrEmpty(postfix))
			{
				return text.Substring(0, maxLength);
			}
			else
			{
				if (maxLength < postfix.Length)
					throw new ArgumentException($"Invalid {nameof(postfix)} length.", nameof(postfix));

				var newLength = maxLength - postfix.Length;
				if (newLength == 0)
					return postfix;
				else 
					return $"{text.SubstringSafe(0, newLength)}{postfix}";
			}
		}

		public static string Replace(string text, Dictionary<string, string> data)
		{
			if (text == null || data == null || data.Count == 0)
				return text;

			foreach (var kvp in data)
				text = text.Replace(kvp.Key, kvp.Value);

			return text;
		}

		public static string RemoveAccents(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return text;

			text = text.Normalize(NormalizationForm.FormD);
			var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
			return new string(chars).Normalize(NormalizationForm.FormC);
		}

		public static string EscapeVerbatimString(string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return str.Replace("\"", "\"\"");
		}

		public static string EscapeString(string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "\\t");
		}

		public static string DelimitString(string value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return value.Contains(Environment.NewLine)
				? "@\"" + EscapeVerbatimString(value) + "\""
				: "\"" + EscapeString(value) + "\"";
		}


		public static string GenerateLiteral(bool value)
			=> value ? "true" : "false";

		public static string GenerateLiteral(byte value)
			=> value.ToString(CultureInfo.InvariantCulture);

		public static string GenerateLiteral(byte[] value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return "new byte[] {" + string.Join(", ", value) + "}";
		}

		public static string GenerateLiteral(sbyte value)
			=> value.ToString(CultureInfo.InvariantCulture);

		public static string GenerateLiteral(short value)
			=> value.ToString(CultureInfo.InvariantCulture);

		public static string GenerateLiteral(ushort value)
			=> value.ToString(CultureInfo.InvariantCulture);

		public static string GenerateLiteral(int value)
			=> value.ToString(CultureInfo.InvariantCulture);

		public static string GenerateLiteral(uint value)
			=> value.ToString(CultureInfo.InvariantCulture);

		public static string GenerateLiteral(char value)
		{
			string stringValue = value.ToString();

			if (value == '\a') stringValue = "\\a";
			else if (value == '\b') stringValue = "\\b";
			else if (value == '\f') stringValue = "\\f";
			else if (value == '\n') stringValue = "\\n";
			else if (value == '\r') stringValue = "\\r";
			else if (value == '\t') stringValue = "\\t";
			else if (value == '\v') stringValue = "\\v";
			else if (value == '\'') stringValue = "\\'";
			else if (value == '\"') stringValue = "\\\"";
			else if (value == '\\') stringValue = "\\\\";

			return "'" + stringValue + "'";
		}

		public static string GenerateLiteral(long value)
			=> value.ToString(CultureInfo.InvariantCulture) + "L";

		public static string GenerateLiteral(ulong value)
			=> value.ToString(CultureInfo.InvariantCulture);

		public static string GenerateLiteral(decimal value)
			=> value.ToString(CultureInfo.InvariantCulture) + "M";

		public static string GenerateLiteral(float value)
			=> value.ToString(CultureInfo.InvariantCulture) + "F";

		public static string GenerateLiteral(double value)
			=> value.ToString(CultureInfo.InvariantCulture) + "D";

		public static string GenerateLiteral(TimeSpan value)
			=> "new TimeSpan(" + value.Ticks + ")";

		public static string GenerateLiteral(DateTime value)
			=> "new DateTime(" + value.Ticks + ", DateTimeKind."
			   + Enum.GetName(typeof(DateTimeKind), value.Kind) + ")";

		public static string GenerateLiteral(DateTimeOffset value)
			=> "new DateTimeOffset(" + value.Ticks + ", "
			   + GenerateLiteral(value.Offset) + ")";

		public static string GenerateLiteral(Guid value)
			=> "new Guid(" + GenerateLiteral(value.ToString()) + ")";

		public static string GenerateLiteral(string value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return "\"" + EscapeString(value) + "\"";
		}

		public static string GenerateVerbatimStringLiteral(string value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return "@\"" + EscapeVerbatimString(value) + "\"";
		}

		public static string GenerateLiteral(object value)
		{
			if (value == null || value == DBNull.Value)
				return "null";

			Type type = value.GetType();
			if (type.GetTypeInfo().IsEnum)
			{
				return type.Name + "." + Enum.Format(type, value, "G");
			}

			if (type == typeof(bool)) return GenerateLiteral((bool)value);
			else if (type == typeof(byte)) return GenerateLiteral((byte)value);
			else if (type == typeof(byte[])) return GenerateLiteral((byte[])value);
			else if (type == typeof(sbyte)) return GenerateLiteral((sbyte)value);
			else if (type == typeof(short)) return GenerateLiteral((short)value);
			else if (type == typeof(ushort)) return GenerateLiteral((ushort)value);
			else if (type == typeof(int)) return GenerateLiteral((int)value);
			else if (type == typeof(uint)) return GenerateLiteral((uint)value);
			else if (type == typeof(char)) return GenerateLiteral((char)value);
			else if (type == typeof(long)) return GenerateLiteral((long)value);
			else if (type == typeof(ulong)) return GenerateLiteral((ulong)value);
			else if (type == typeof(decimal)) return GenerateLiteral((decimal)value);
			else if (type == typeof(float)) return GenerateLiteral((float)value);
			else if (type == typeof(double)) return GenerateLiteral((double)value);
			else if (type == typeof(TimeSpan)) return GenerateLiteral((TimeSpan)value);
			else if (type == typeof(DateTime)) return GenerateLiteral((DateTime)value);
			else if (type == typeof(DateTimeOffset)) return GenerateLiteral((DateTimeOffset)value);
			else if (type == typeof(Guid)) return GenerateLiteral((Guid)value);
			else if (type == typeof(string)) return GenerateLiteral((string)value);

			return string.Format(CultureInfo.InvariantCulture, "{0}", value);
		}

		public static string BeautifyJson(string json)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1
			var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
			var formatted = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
			return formatted;
#elif NET5_0
			using var document = System.Text.Json.JsonDocument.Parse(json);
			using var stream = new System.IO.MemoryStream();

			//var testSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All);

			using var writer =
				new System.Text.Json.Utf8JsonWriter(
					stream,
					new System.Text.Json.JsonWriterOptions
					{
						Indented = true,
						Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
					});

			document.WriteTo(writer);
			writer.Flush();
			return Encoding.UTF8.GetString(stream.ToArray());
#endif
		}

		/// <summary>
		/// ReduceWhitespaces
		/// </summary>
		/// <param name="value">Input string</param>
		/// <param name="reduceToWhitespace">All whitespace replaces with this char. If null, first whitespace will be used</param>
		/// <returns>String without multiple whitespaces</returns>
		public static string ReduceWhitespaces(string value, char? reduceToWhitespace = ' ')
		{
			var newString = new StringBuilder();
			bool previousIsWhitespace = false;
			for (int i = 0; i < value.Length; i++)
			{
				var val = value[i];
				if (char.IsWhiteSpace(val))
				{
					if (previousIsWhitespace)
						continue;

					previousIsWhitespace = true;

					if (reduceToWhitespace.HasValue)
						val = reduceToWhitespace.Value;
				}
				else
				{
					previousIsWhitespace = false;
				}

				newString.Append(val);
			}

			return newString.ToString();
		}

		[return: NotNullIfNotNull("source")]
		[return: NotNullIfNotNull("text")]
		public static string? ConcatIfNotNullOrEmpty(this string? source, string? text)
		{
			if (string.IsNullOrEmpty(source))
				return text ?? source;

			if (string.IsNullOrEmpty(text))
				return source ?? text;

			return string.Concat(source, text);
		}

		[return: NotNullIfNotNull("source")]
		[return: NotNullIfNotNull("text")]
		public static string? ConcatIfNotNullOrEmpty(this string? source, string delimiter, string? text)
		{
			if (string.IsNullOrEmpty(source))
				return text ?? source;

			if (string.IsNullOrEmpty(text))
				return source ?? text;

			return string.Concat(source, delimiter, text);
		}

		[return: NotNullIfNotNull("values")]
		public static string? ConcatIfNotNullOrEmpty(string delimiter, IEnumerable<string?> values)
		{
			if (values == null)
				return null;

			if (!values.Any())
				return string.Empty;

			var sb = new StringBuilder();
			bool empty = true;

			foreach (var value in values)
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (!empty)
						sb.Append(delimiter);

					sb.Append(value);
					empty = false;
				}
			}

			return sb.ToString();
		}

		[return: NotNullIfNotNull("values")]
		public static string? ConcatIfNotNullOrEmpty(string delimiter, params string?[] values)
			=> ConcatIfNotNullOrEmpty(delimiter, (IEnumerable<string?>)values);
	}
}
