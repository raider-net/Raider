using System;
using System.IO;
using System.Text;
#if NETSTANDARD2_0 || NETSTANDARD2_1
using Newtonsoft.Json;
#elif NET5_0_OR_GREATER
using System.Text.Json;
#endif

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

#if NETSTANDARD2_0 || NETSTANDARD2_1
				var jsonSerializerSettings = new JsonSerializerSettings
				{
					Formatting = Formatting.Indented,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
					PreserveReferencesHandling = PreserveReferencesHandling.Objects, //PreserveReferencesHandling.All,
					TypeNameHandling = TypeNameHandling.All
				};
#elif NET5_0_OR_GREATER
				var jsonSerializerOptions = new JsonSerializerOptions
				{
					WriteIndented = true,
					ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
				};
#endif

				foreach (var item in ex.Data.Keys)
				{
					var obj = ex.Data[item];
					string key = "";
					string value = "";
					try
					{
#if NETSTANDARD2_0 || NETSTANDARD2_1
						key = JsonConvert.SerializeObject(item, jsonSerializerSettings);
#elif NET5_0_OR_GREATER
						key = JsonSerializer.Serialize(item, jsonSerializerOptions);
#endif
					}
					catch { }
					try
					{
#if NETSTANDARD2_0 || NETSTANDARD2_1
						value = JsonConvert.SerializeObject(obj, jsonSerializerSettings);
#elif NET5_0_OR_GREATER
						value = JsonSerializer.Serialize(obj, jsonSerializerOptions);
#endif
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
