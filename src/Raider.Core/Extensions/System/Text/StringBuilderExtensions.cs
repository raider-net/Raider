using System;
using System.Text;

namespace Raider.Extensions
{
	public static class StringBuilderExtensions
	{
		public static StringBuilder AppendSafe(this StringBuilder sb, string? text)
		{
			if (sb == null)
				throw new ArgumentNullException(nameof(sb));

			if (!string.IsNullOrEmpty(text))
				sb.Append(text);

			return sb;
		}

		public static StringBuilder AppendSafe(this StringBuilder sb, string? text, Func<string>? defaultText)
		{
			if (sb == null)
				throw new ArgumentNullException(nameof(sb));

			if (!string.IsNullOrEmpty(text))
			{
				sb.Append(text);
			}
			else
			{
				var def = defaultText?.Invoke();
				if (!string.IsNullOrEmpty(def))
					sb.Append(def);
			}

			return sb;
		}

		public static StringBuilder AppendSafe(this StringBuilder sb, bool condition, Func<string>? text, Func<string>? defaultText = null)
		{
			if (sb == null)
				throw new ArgumentNullException(nameof(sb));

			if (condition)
			{
				var t = text?.Invoke();
				if (!string.IsNullOrEmpty(t))
					sb.Append(t);
			}
			else
			{
				var def = defaultText?.Invoke();
				if (!string.IsNullOrEmpty(def))
					sb.Append(def);
			}

			return sb;
		}

		public static StringBuilder AppendLineSafe(this StringBuilder sb, string? text)
		{
			if (sb == null)
				throw new ArgumentNullException(nameof(sb));

			if (!string.IsNullOrEmpty(text))
				sb.AppendLine(text);

			return sb;
		}

		public static StringBuilder AppendLineSafe(this StringBuilder sb, string? text, Func<string>? defaultText)
		{
			if (sb == null)
				throw new ArgumentNullException(nameof(sb));

			if (!string.IsNullOrEmpty(text))
			{
				sb.AppendLine(text);
			}
			else
			{
				var def = defaultText?.Invoke();
				if (!string.IsNullOrEmpty(def))
					sb.AppendLine(def);
			}

			return sb;
		}

		public static StringBuilder AppendLineSafe(this StringBuilder sb, bool condition, Func<string>? text, Func<string>? defaultText = null)
		{
			if (sb == null)
				throw new ArgumentNullException(nameof(sb));

			if (condition)
			{
				var t = text?.Invoke();
				if (!string.IsNullOrEmpty(t))
					sb.AppendLine(t);
			}
			else
			{
				var def = defaultText?.Invoke();
				if (!string.IsNullOrEmpty(def))
					sb.AppendLine(def);
			}

			return sb;
		}
	}
}
