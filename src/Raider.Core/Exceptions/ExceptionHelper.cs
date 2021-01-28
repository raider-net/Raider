using System;
using System.IO;
using System.Text;

namespace Raider.Exceptions
{
	public static class ExceptionHelper
	{
		public static string ToStringTrace(Exception ex)
		{
			if (ex == null)
				return "";

			StringBuilder sb = new StringBuilder(ex.ToString());

			if (ex is System.Reflection.ReflectionTypeLoadException rtlEx && 0 < rtlEx.LoaderExceptions.Length)
			{
				sb.AppendLine();
				sb.AppendLine("--- LoaderExceptions ---");
				foreach (Exception? exSub in rtlEx.LoaderExceptions)
				{
					if (exSub != null)
					{
						sb.AppendLine(exSub.ToString());

						if (exSub is FileNotFoundException exFileNotFound)
							if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
							{
								sb.AppendLine("Fusion Log:");
								sb.AppendLine(exFileNotFound.FusionLog);
							}

						sb.AppendLine();
					}
				}
			}

			if (ex.Data != null && 0 < ex.Data.Count)
			{
				sb.AppendLine();
				sb.AppendLine("--- DATA ---");
				var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
				{
					WriteIndented = true,
					ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
				};
				foreach (var item in ex.Data.Keys)
				{
					var obj = ex.Data[item];
					string key = "";
					string value = "";
					try
					{
						key = System.Text.Json.JsonSerializer.Serialize(item, jsonSerializerOptions);
					}
					catch { }
					try
					{
						value = System.Text.Json.JsonSerializer.Serialize(obj, jsonSerializerOptions);
					}
					catch { }

					sb.AppendLine($"{key}: {value}");
				}
			}

			//if (ex is ISerializableException serializableException)
			//{
			//	sb.AppendLine();
			//	sb.AppendLine("--- ISerializableException ---");
			//	sb.AppendLine(serializableException.Serialize());
			//}

			try
			{
				var tmp = RaiderConfiguration.SerializeFaultException;
				if (tmp != null)
					tmp.Invoke(sb, ex);
			}
			catch { }

			return sb.ToString();
		}
	}
}
