using Raider.MathUtils;
using Raider.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Raider.Extensions
{
	public static class StringExtensions
	{
		public static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

		public static byte[] StringToBytes(this string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}

			byte[] bytes = new byte[text.Length * sizeof(char)];
			System.Buffer.BlockCopy(text.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		public static string EncodeToBase64(this string text, System.Text.Encoding encoding = null)
		{
			if (encoding == null) encoding = System.Text.Encoding.ASCII;
			byte[] toEncodeAsBytes = encoding.GetBytes(text);
			string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
			return returnValue;
		}

		public static string DecodeFromBase64(this string base64Text, System.Text.Encoding encoding = null)
		{
			if (encoding == null) encoding = System.Text.Encoding.ASCII;
			byte[] encodedDataAsBytes = System.Convert.FromBase64String(base64Text);
			string returnValue = encoding.GetString(encodedDataAsBytes);
			return returnValue;
		}

		public static string DefaultIfNull(this string text, string defaultString = null)
		{
			return text ?? defaultString;
		}

		public static string DefaultIfNullOrEmpty(this string text, string defaultString = null)
		{
			return string.IsNullOrEmpty(text) ? defaultString : text;
		}

		public static string DefaultIfNullOrWhiteSpace(this string text, string defaultString = null)
		{
			return string.IsNullOrWhiteSpace(text) ? defaultString : text;
		}

		public static string RemoveAccents(this string text)
			=> StringHelper.RemoveAccents(text);

		public static bool IsSyntacticSimilar(this string s, string text, double threshold = 0.7, bool ignoreCase = true, bool ignoreAccent = true, System.Globalization.CultureInfo ignoreCaseCultureInfo = null, int searchRange = 1)
		{
			return threshold <= GetSyntacticMatchingScore(s, text, ignoreCase, ignoreAccent, ignoreCaseCultureInfo, searchRange);
		}

		public static double GetSyntacticMatchingScore(this string s, string text, bool ignoreCase, bool ignoreAccent, System.Globalization.CultureInfo ignoreCaseCultureInfo, int searchRange)
		{
			if (s == null)
			{
				if (text == null)
				{
					return 1.0;
				}
				else
				{
					s = string.Empty;
				}
			}
			else
			{
				if (text == null)
				{
					text = string.Empty;
				}
			}

			if (ignoreCase)
			{
				if (ignoreCaseCultureInfo != null)
				{
					s = s.ToLower(ignoreCaseCultureInfo);
					text = text.ToLower(ignoreCaseCultureInfo);
				}
				else
				{
					s = s.ToLower();
					text = text.ToLower();
				}
			}

			if (ignoreAccent)
			{
				s = s.RemoveAccents();
				text = text.RemoveAccents();
			}

			double j1 = GetCommonMatches(s, text, searchRange);
			double j2 = GetCommonMatches(text, s, searchRange);
			double j = (j1 + j2) / 2;
			return j;
		}

		private static double GetCommonMatches(string firstWord, string secondWord, int searchRange)
		{
			int searchRange1 = searchRange + 1;
			int firstLen = firstWord.Length;
			int secondLen = secondWord.Length;
			double[] rating = new double[firstLen];
			double[] charUsage = new double[secondLen];

			for (int i = 0; i < firstLen; i++)
			{
				char ch = firstWord[i];
				int start = Math.Max(0, i - searchRange);
				int end = Math.Min(i + searchRange, secondLen - 1);
				for (int j = start; j <= end; j++)
				{
					double freeCharUsage = 1 - charUsage[j];
					if (0 < freeCharUsage && secondWord[j] == ch)
					{
						if (i == j) //100% match
						{
							rating[i] = freeCharUsage;
							charUsage[j] += 1; //100% using
						}
						else
						{
							double actualRating = (searchRange1 - Math.Abs(i - j)) / searchRange1; //ak su prehodene pismenka v poradi. Rating zavisi od vzdialenosti [j] od pozicie [i]
							rating[i] = actualRating;
							charUsage[j] += (actualRating < freeCharUsage) ? actualRating : freeCharUsage;
						}
						break;
					}
				}
			}

			double result = 0.0;
			for (int i = 0; i < firstLen; i++)
			{
				result = result + rating[i];
			}

			return result / firstLen;
		}

		/// <summary>
		/// Jaro-Winkler distance
		/// true ak JaroWinklerDistance >= similarityThreshold, inak false
		/// </summary>
		public static bool IsSimilarByJaroWinkler(this string s, string text, double similarityThreshold = 0.7, bool ignoreCase = true, bool ignoreAccent = true, System.Globalization.CultureInfo ignoreCaseCultureInfo = null, double bonusThreshold = 0.7, Nullable<int> searchRange = 1, bool useWinklekBonus = true)
		{
			return similarityThreshold <= s.GetJaroWinklerDistance(text, ignoreCase, ignoreAccent, ignoreCaseCultureInfo, bonusThreshold, searchRange, useWinklekBonus);
		}

		public static double GetJaroWinklerDistance(this string s, string text, bool ignoreCase, bool ignoreAccent, System.Globalization.CultureInfo ignoreCaseCultureInfo, double bonusThreshold, Nullable<int> searchRange, bool useWinklekBonus)
		{
			if (s == null)
			{
				if (text == null)
				{
					return 1.0;
				}
				else
				{
					s = string.Empty;
				}
			}
			else
			{
				if (text == null)
				{
					text = string.Empty;
				}
			}

			if (ignoreCase)
			{
				if (ignoreCaseCultureInfo != null)
				{
					s = s.ToLower(ignoreCaseCultureInfo);
					text = text.ToLower(ignoreCaseCultureInfo);
				}
				else
				{
					s = s.ToLower();
					text = text.ToLower();
				}
			}

			if (ignoreAccent)
			{
				s = s.RemoveAccents();
				text = text.RemoveAccents();
			}

			if (s == text)
			{
				return 1.0;
			}
			else
			{
				int halflen = searchRange ?? Math.Min(s.Length, text.Length) / 2 + 1;
				StringBuilder common1 = GetJaroCommonCharacters(s, text, halflen);
				int commonMatches = common1.Length;
				if (commonMatches == 0)
				{
					return 0.0;
				}

				StringBuilder common2 = GetJaroCommonCharacters(text, s, halflen);
				if (commonMatches != common2.Length)
				{
					return 0.0;
				}

				int transpositions = 0;
				for (int i = 0; i < commonMatches; i++)
				{
					if (common1[i] != common2[i])
					{
						transpositions++;
					}
				}

				transpositions /= 2;
				double jaro1 = commonMatches / (3.0 * s.Length);
				double jaro2 = commonMatches / (3.0 * text.Length);
				double jaro3 = (commonMatches - transpositions) / (3.0 * commonMatches);
				double jaroDistance = jaro1 + jaro2 + jaro3;

				if (!useWinklekBonus || jaroDistance < bonusThreshold)
				{
					return jaroDistance;
				}

				int prefixLength = GetPrefixLength(s, text);
				double jaroWinklerDistance = jaroDistance + (Math.Min(4, prefixLength) * 0.1 * (1 - jaroDistance));
				return jaroWinklerDistance;
			}
		}

		private static StringBuilder GetJaroCommonCharacters(string firstWord, string secondWord, int distanceSep)
		{
			StringBuilder returnCommons = new StringBuilder(20);
			StringBuilder copy = new StringBuilder(secondWord);

			int firstLen = firstWord.Length;
			int secondLen = secondWord.Length;
			bool[] charUsage = new bool[secondLen];

			for (int i = 0; i < firstLen; i++)
			{
				char ch = firstWord[i];
				int start = Math.Max(0, i - distanceSep);
				int end = Math.Min(i + distanceSep, secondLen - 1);
				for (int j = start; j <= end; j++)
				{
					if (!charUsage[j] && copy[j] == ch)
					{
						returnCommons.Append(ch);
						charUsage[j] = true;
						break;
					}
				}
			}
			return returnCommons;
		}

		private static int GetPrefixLength(string firstWord, string secondWord)
		{
			int n = Math.Min(firstWord.Length, secondWord.Length);
			for (int i = 0; i < n; i++)
			{
				if (firstWord[i] != secondWord[i]) return i;
			}
			return n;
		}

		/// <summary>
		/// Replace chars in string
		/// </summary>
		public static string ReplaceAll(this string seed, char[] chars, string replacementString)
		{
			return chars.Aggregate(seed, (str, cItem) => str.Replace(cItem.ToString(), replacementString));
		}

		public static string Replace(this string text, Dictionary<string, string> data)
			=> StringHelper.Replace(text, data);

		public static string ConvertToEncoding(this string text, Encoding sourceEncoding, Encoding targetEncoding)
		{
			if (text == null) return null;
			if (sourceEncoding == null || targetEncoding == null) return text;
			try
			{
				byte[] bytes = sourceEncoding.GetBytes(text);
				return targetEncoding.GetString(bytes);
			}
			catch (System.Exception)
			{
				return text;
			}
		}

		public static string FirstToUpper(this string s)
		{
			/*if (s.Length == 0) return string.Empty;

            return Regex.Replace(s, @"\b[a-z]\w+", delegate(Match match)
            {
                string v = match.ToString();
                return char.ToUpper(v[0]) + v.Substring(1);
            });*/
			if (string.IsNullOrWhiteSpace(s)) return s;
			if (s.Length == 1)
			{
				return s.ToUpper();
			}
			else
			{
				return char.ToUpper(s[0]) + s.Substring(1);
			}
		}

		public static string FirstToUpper(this string s, CultureInfo cultureInfo)
		{
			/*if (s.Length == 0) return string.Empty;

            return Regex.Replace(s, @"\b[a-z]\w+", delegate(Match match)
            {
                string v = match.ToString();
                return char.ToUpper(v[0]) + v.Substring(1);
            });*/
			if (string.IsNullOrWhiteSpace(s)) return s;
			if (s.Length == 1)
			{
				return s.ToUpper(cultureInfo);
			}
			else
			{
				return char.ToUpper(s[0], cultureInfo) + s.Substring(1);
			}
		}

		public static int IndexOfFast(this string source, string pattern)
		{
			if (pattern == null) return -1;
			if (pattern.Length == 0) return 0;
			if (pattern.Length == 1) return source.IndexOf(pattern[0]);
			bool found;
			int limit = source.Length - pattern.Length + 1;
			if (limit < 1) return -1;
			// Store the first 2 characters of "pattern"
			char c0 = pattern[0];
			char c1 = pattern[1];
			// Find the first occurrence of the first character
			int first = source.IndexOf(c0, 0, limit);
			while (first != -1)
			{
				// Check if the following character is the same like
				// the 2nd character of "pattern"
				if (source[first + 1] != c1)
				{
					first = source.IndexOf(c0, ++first, limit - first);
					continue;
				}
				// Check the rest of "pattern" (starting with the 3rd character)
				found = true;
				for (int j = 2; j < pattern.Length; j++)
					if (source[first + j] != pattern[j])
					{
						found = false;
						break;
					}
				// If the whole word was found, return its index, otherwise try again
				if (found) return first;
				first = source.IndexOf(c0, ++first, limit - first);
			}
			return -1;
		}

		public static int IndexOfFast(this string source, string pattern, int startIndex)
		{
			if (pattern == null || startIndex < 0) return -1;
			if (startIndex == 0) return IndexOfFast(source, pattern);
			if (source.Length <= startIndex) return -1;
			if (pattern.Length == 0) return 0;
			if (source.Length <= startIndex + pattern.Length) return -1;
			if (pattern.Length == 1) return source.IndexOf(pattern[0], startIndex);
			bool found;
			int limit = source.Length - pattern.Length + 1 - startIndex;
			if (limit < 1) return -1;
			// Store the first 2 characters of "pattern"
			char c0 = pattern[0];
			char c1 = pattern[1];
			// Find the first occurrence of the first character
			int first = source.IndexOf(c0, startIndex, limit);
			while (first != -1)
			{
				// Check if the following character is the same like
				// the 2nd character of "pattern"
				if (source[first + 1] != c1)
				{
					first = source.IndexOf(c0, ++first, limit - first + startIndex);
					continue;
				}
				// Check the rest of "pattern" (starting with the 3rd character)
				found = true;
				for (int j = 2; j < pattern.Length; j++)
					if (source[first + j] != pattern[j])
					{
						found = false;
						break;
					}
				// If the whole word was found, return its index, otherwise try again
				if (found) return first;
				first = source.IndexOf(c0, ++first, limit - first + startIndex);
			}
			return -1;
		}

		public static string ExtractBetween(this string searchString, string leftString, string rightString)
		{
			return ExtractBetween(searchString, leftString, rightString, 0, out int left, out int right);
		}

		public static string ExtractBetween(this string searchString, string leftString, string rightString, int startIndex)
		{
			return ExtractBetween(searchString, leftString, rightString, startIndex, out int left, out int right);
		}

		public static string ExtractBetween(this string searchString, string leftString, string rightString, int startIndex, out int leftIndex, out int rightIndex)
		{
			//Regex regex = new Regex(string.Format("{0}(.*){1}", leftString, rightString));
			//var v = regex.Match(searchString);
			//return v.Groups[1].ToString();

			int leftIdx = startIndex == 0 ? searchString.IndexOfFast(leftString) : searchString.IndexOfFast(leftString, startIndex);
			if (leftIdx < 0)
			{
				leftIndex = -1;
				rightIndex = -1;
				return null;
			}
			int rightIdx = searchString.IndexOfFast(rightString, leftIdx + 1);
			if (rightIdx < 0)
			{
				leftIndex = -1;
				rightIndex = -1;
				return null;
			}
			int fromIndex = leftIdx + leftString.Length;
			int length = rightIdx - fromIndex;
			if (length < 1)
			{
				leftIndex = fromIndex;
				rightIndex = fromIndex;
				return string.Empty;
			}
			leftIndex = fromIndex;
			rightIndex = rightIdx - 1;
			return searchString.Substring(fromIndex, length);
		}

		public static string BetweenInclude(this string searchString, string leftString, string rightString)
		{
			return string.Concat(leftString, searchString.ExtractBetween(leftString, rightString), rightString);
		}

		public static string ToCultureSafeDecimalString(this string text, string defaultSeparator = ".")
		{
			if (string.IsNullOrWhiteSpace(text)) return string.Empty;
			string separator = MathHelper.GetDecimalSeparator();
			if (separator != null)
			{
				return text.Trim().Replace(",", separator).Replace(".", separator);
			}
			else
			{
				return text.Trim().Replace(",", defaultSeparator);
			}
		}

		public static string ToMd5Fingerprint(this string s)
		{
			var bytes = Encoding.Unicode.GetBytes(s.ToCharArray());
			var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);

			// concat the hash bytes into one long string
			return hash.Aggregate(new StringBuilder(32),
				(sb, b) => sb.Append(b.ToString("X2")))
				.ToString();
		}

		public static List<string> ToList(this string s, string delimiter)
		{
			if (s == null) return null;
			string[] items = s.Split(new string[] { delimiter }, StringSplitOptions.None);
			if (items == null || items.Length == 0) return null;
			return items.ToList();
		}

		//public static bool Contains(this string source, string value, StringComparison comp)
		//{
		//    return source.IndexOf(value, comp) >= 0;
		//}

		public static int IndexOfSafe(this string text, string value, int startIndex)
		{
			if (string.IsNullOrEmpty(text))
			{
				return -1;
			}
			if (startIndex < text.Length)
			{
				return text.IndexOfFast(value, startIndex);
			}
			else
			{
				return -1;
			}
		}

		public static int LastIndexOfSafe(this string text, string value, int startIndex)
		{
			int result = IndexOfSafe(text, value, startIndex);
			int lastIndex = result;
			while (-1 < lastIndex)
			{
				lastIndex = IndexOfSafe(text, value, lastIndex + 1);
				if (result < lastIndex)
				{
					result = lastIndex;
				}
			}
			return result;
		}

		public static string SubstringSafe(this string text, int startIndex)
		{
			if (string.IsNullOrEmpty(text) || startIndex < 0)
			{
				return text;
			}

			if (text.Length - 1 < startIndex)
			{
				return "";
			}
			else
			{
				if (startIndex == 0)
					return text;
				else
					return text.Substring(startIndex);
			}
		}

		public static string SubstringSafe(this string text, int startIndex, int length)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			if (startIndex < 0)
			{
				if (length < 1)
				{
					return "";
				}
				else if (startIndex + length < 0)
				{
					return "";
				}
				else if (text.Length < startIndex + length)
				{
					return text;
				}
				else
				{
					return text.Substring(0, startIndex + length);
				}
			}
			else if (text.Length - 1 < startIndex)
			{
				return "";
			}
			else if (length < 1)
			{
				return "";
			}
			else if (text.Length < startIndex + length)
			{
				if (startIndex == 0)
					return text;
				else
					return text.Substring(startIndex);
			}
			else
			{
				return text.Substring(startIndex, length);
			}
		}

		public static string SubstringRange(this string text, int startIndex, int endIndex)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			if (endIndex < startIndex)
			{
				return "";
			}

			if (startIndex < 0)
			{
				if (endIndex < 0)
				{
					return "";
				}
				else if (text.Length - 1 <= endIndex)
				{
					return text;
				}
				else
				{
					return text.SubstringSafe(0, endIndex + 1);
				}
			}
			else if (text.Length - 1 < startIndex)
			{
				return "";
			}
			else if (endIndex < 1)
			{
				return "";
			}
			else if (text.Length - 1 <= endIndex)
			{
				return text.Substring(startIndex);
			}
			else
			{
				return text.SubstringSafe(startIndex, endIndex - startIndex + 1);
			}
		}

		public static bool StartsWithSafe(this string text, string? value)
			=> value != null && text.StartsWith(value);

		public static bool EndsWithSafe(this string text, string? value)
			=> value != null && text.EndsWith(value);

		public static bool ContainsSafe(this string text, string? value)
			=> value != null && text.Contains(value);

		public static string ReplaceAll(this string text, string left, string right, bool replaceFirstRight, string replceWith, bool replaceLeftRight)
		{
			string result = text;
			int startIndex = 0;
			int lastIndex = 0;
			while (-1 < lastIndex)
			{
				result = ReplaceOne(result, left, right, replaceFirstRight, replceWith, replaceLeftRight, startIndex, out lastIndex);
				startIndex = lastIndex;
			}
			return result;
		}

		public static string ReplaceAll(this string text, string left, string right, bool replaceFirstRight, string replceWith, bool replaceLeftRight, int startIndex)
		{
			string result = text;
			if (startIndex < 0) startIndex = 0;
			int lastIndex = 0;
			while (-1 < lastIndex)
			{
				result = ReplaceOne(result, left, right, replaceFirstRight, replceWith, replaceLeftRight, startIndex, out lastIndex);
				startIndex = lastIndex;
			}
			return result;
		}

		public static string ReplaceFirst(this string text, string left, string right, bool replaceFirstRight, string replceWith, bool replaceLeftRight)
		{
			return ReplaceOne(text, left, right, replaceFirstRight, replceWith, replaceLeftRight, 0, out int lastIndex);
		}

		public static string ReplaceFirst(this string text, string left, string right, bool replaceFirstRight, string replceWith, bool replaceLeftRight, int startIndex)
		{
			if (startIndex < 0) startIndex = 0;
			return ReplaceOne(text, left, right, replaceFirstRight, replceWith, replaceLeftRight, startIndex, out int lastIndex);
		}

		private static string ReplaceOne(string text, string left, string right, bool replaceFirstRight, string replceWith, bool replaceLeftRight, int startIndex, out int lastIndex)
		{
			if (string.IsNullOrEmpty(text))
			{
				lastIndex = -1;
				return text;
			}
			if (!string.IsNullOrEmpty(left))
			{
				int leftIndexBegin = 0 < startIndex ? text.IndexOfSafe(left, startIndex) : text.IndexOfFast(left);
				if (leftIndexBegin < 0)
				{
					lastIndex = -1;
					return text;
				}
				else
				{
					int leftIndexEnd = leftIndexBegin + left.Length - 1;
					if (!string.IsNullOrEmpty(right))
					{
						int rightIndexBegin = -1;
						if (replaceFirstRight)
						{
							rightIndexBegin = replaceLeftRight ? text.IndexOfSafe(right, leftIndexBegin + 1) : text.IndexOfSafe(right, leftIndexEnd + 1);
						}
						else
						{
							rightIndexBegin = replaceLeftRight ? text.LastIndexOfSafe(right, leftIndexBegin + 1) : text.LastIndexOfSafe(right, leftIndexEnd + 1);
						}

						if (rightIndexBegin < 0)
						{
							lastIndex = -1;
							return text;
						}
						else
						{
							int rightIndexEnd = rightIndexBegin + right.Length - 1;
							string str1 = replaceLeftRight ? text.Substring(0, leftIndexBegin) : text.Substring(0, leftIndexEnd + 1);
							lastIndex = replaceLeftRight ? rightIndexEnd + 1 : rightIndexBegin;
							string str2 = text.SubstringSafe(lastIndex);
							if (string.IsNullOrEmpty(str2))
							{
								return string.Concat(str1, replceWith);
							}
							else
							{
								return string.Concat(str1, replceWith, str2);
							}
						}
					}
					else
					{
						lastIndex = replaceLeftRight ? leftIndexBegin : leftIndexEnd + 1;
						string str1 = text.Substring(0, lastIndex);
						return string.Concat(str1, replceWith);
					}
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(right))
				{
					int rightIndexBegin = -1;
					if (replaceFirstRight)
					{
						rightIndexBegin = 0 < startIndex ? text.IndexOfSafe(right, startIndex) : text.IndexOfFast(right);
					}
					else
					{
						rightIndexBegin = 0 < startIndex ? text.LastIndexOfSafe(right, startIndex) : text.IndexOfFast(right);
					}
					if (rightIndexBegin < 0)
					{
						lastIndex = -1;
						return text;
					}
					else
					{
						int rightIndexEnd = rightIndexBegin + right.Length - 1;
						lastIndex = replaceLeftRight ? rightIndexEnd + 1 : rightIndexBegin;
						string str2 = text.SubstringSafe(lastIndex);
						if (string.IsNullOrEmpty(str2))
						{
							return replceWith;
						}
						else
						{
							return string.Concat(replceWith, str2);
						}
					}
				}
				else
				{
					lastIndex = -1;
					return text;
				}
			}
		}

		public static string RemoveWhitespaces(this string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			return new string(text.ToCharArray()
				.Where(c => !Char.IsWhiteSpace(c))
				.ToArray());
		}

		public static List<CharInfo> GetCharInfos(this string text)
		{
			return StringHelper.GetCharInfos(text);
		}

		public static string TrimPrefix(this string text, string prefix, bool ignoreCase = false)
		{
			return StringHelper.TrimPrefix(text, prefix, ignoreCase);
		}

		public static string TrimPostfix(this string text, string postfix, bool ignoreCase = false)
		{
			return StringHelper.TrimPostfix(text, postfix, ignoreCase);
		}

		public static string? TrimLength(this string text, int maxLength, string? postfix = null)
		{
			return StringHelper.TrimLength(text, maxLength, postfix);
		}

		public static string ToCammelCase(this string text, bool strictCammelCase = false, bool removeUnderscores = true, bool throwIfEmpty = true)
		{
			return StringHelper.ToCammelCase(text, strictCammelCase, removeUnderscores, throwIfEmpty);
		}

		public static MemoryStream ToMemoryStream(this string text, Encoding encoding = null)
		{
			if (encoding == null)
				encoding = Encoding.UTF8;

			return new MemoryStream(
						text == null
							? encoding.GetBytes("")
							: encoding.GetBytes(text));
		}

		public static string FirstToLower(this string text, bool allFirstToLower = false)
		{
			if (string.IsNullOrWhiteSpace(text)) return text;
			if (text.Length == 1)
			{
				return text.ToLower();
			}
			else if (allFirstToLower)
			{
				string up = text.ToUpper();
				string toLower = text[0].ToString();
				int index = 0;
				for (int i = 1; i < text.Length; i++)
				{
					if (up[i] == text[i])
					{
						toLower = string.Format("{0}{1}", toLower, text[i]);
						index = i;
					}
					else
					{
						break;
					}
				}
				if (0 < index) toLower = toLower.Substring(0, toLower.Length);
				index = index + 1;
				return toLower.ToLower() + text.SubstringSafe(index);
			}
			else
			{
				return char.ToLower(text[0]) + text.Substring(1);
			}
		}

		public static string FirstToLower(this string s, CultureInfo cultureInfo, bool allFirstToLower = false)
		{
			if (string.IsNullOrWhiteSpace(s)) return s;
			if (s.Length == 1)
			{
				return s.ToLower(cultureInfo);
			}
			else if (allFirstToLower)
			{
				string up = s.ToUpper(cultureInfo);
				string toLower = s[0].ToString();
				int index = 0;
				for (int i = 1; i < s.Length; i++)
				{
					if (up[i] == s[i])
					{
						toLower = string.Format("{0}{1}", toLower, s[i]);
						index = i;
					}
					else
					{
						break;
					}
				}
				if (0 < index) toLower = toLower.Substring(0, toLower.Length - 1);
				else index = index + 1;
				return toLower.ToLower(cultureInfo) + s.Substring(index);
			}
			else
			{
				return char.ToLower(s[0], cultureInfo) + s.Substring(1);
			}
		}

		public static string GetLastSplitSubstring(this string s, string delimiter)
		{
			if (s == null)
				throw new ArgumentNullException(nameof(s));

			string[] items = s.Split(new string[] { delimiter }, StringSplitOptions.None);
			return items.Last();
		}

		[return: NotNullIfNotNull("template")]
		public static string? ReplacePlaceholders(this string template, IDictionary<string, object?> values)
			=> TemplateFormatter.ReplacePlaceholders(template, values);

		[return: NotNullIfNotNull("text")]
		public static string? ToXmlEscapedValueString(this string text)
			=> StringHelper.ToXmlValueString(text);
	}
}
