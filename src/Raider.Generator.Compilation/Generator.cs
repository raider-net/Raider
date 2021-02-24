using Raider.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Raider.Generator.Compilation
{
	public class Generator
	{
		public static void UpdateResource(
			IEnumerable<string> keys,
			IEnumerable<string> keyFormatters,
			string activitiesResxFilePath)
		{
			var keysList = keys?.ToList() ?? throw new ArgumentNullException(nameof(keys));
			var keyFormattersList = keyFormatters?.ToList() ?? throw new ArgumentNullException(nameof(keyFormatters));

			for (int i = 0; i < keysList.Count - 1; i++)
			{
				for (int j = i + 1; j < keysList.Count; j++)
				{
					if (string.IsNullOrWhiteSpace(keysList[i]))
						throw new InvalidOperationException($"Empty key at index = {i}. {keysList[i]}");
					if (string.IsNullOrWhiteSpace(keysList[j]))
						throw new InvalidOperationException($"Empty key at index = {j}. {keysList[j]}");

					if (keysList[i].Equals(keysList[j], StringComparison.OrdinalIgnoreCase))
						throw new InvalidOperationException($"Not UNIQUE key {keysList[i].ToString()} on {i} and {j}");
				}
			}

			var resxBuilder = new ResxBuilder(activitiesResxFilePath);
			List<ResxData> dataList = new List<ResxData>();

			foreach (var key in keysList)
			{
				if (keyFormattersList == null || keyFormattersList.Count == 0)
				{
					var existing = resxBuilder.Data.FirstOrDefault(data => data.Name == key);
					if (existing == null)
						dataList.Add(new ResxData(key, key));
					else
						dataList.Add(existing);
				}
				else
				{
					foreach (var keyFormatter in keyFormattersList)
					{
						string resxKey = string.Format(keyFormatter, key);
						var existing = resxBuilder.Data.FirstOrDefault(data => data.Name == resxKey);
						if (existing == null)
							dataList.Add(new ResxData(resxKey, key));
						else
							dataList.Add(existing);
					}
				}
			}

			resxBuilder.Clear();
			foreach (var data in dataList)
				resxBuilder.Add(data);

			resxBuilder.Serialize();
		}

		public static void GenerateResources(
			string targetProject,
			string rootNamespace,
			Assembly assembly,
			CultureInfo defaultCulture,
			string resourcesClassName)
		{
			if (string.IsNullOrWhiteSpace(targetProject))
				throw new ArgumentNullException(nameof(targetProject));

			if (!targetProject.EndsWith("\\"))
			{
				targetProject += "\\";
			}

			var resFiles = ResourceLoader.LoadResources(targetProject, assembly, defaultCulture, ResourceLoadOptions.LoadResxAllResources, SearchOption.AllDirectories);

			ResourcesGenerator resourcesGenerator = new ResourcesGenerator
			{
				WriteMode = GeneratorBase.WriteModes.Overwritte
			};
			resourcesGenerator.SetParam("TargetProject", targetProject);
			resourcesGenerator.SetParam("RootNamespace", rootNamespace);
			resourcesGenerator.SetParam("ResFiles", resFiles);
			resourcesGenerator.SetParam("ResourcesClassName", resourcesClassName);
			resourcesGenerator.TransformText();
			var errors = resourcesGenerator.ErrorString();
			if (!string.IsNullOrWhiteSpace(errors))
				throw new Exception(errors);
		}

		public static void GenerateResourceKeys(
			string targetProject,
			string rootNamespace,
			Assembly assembly,
			CultureInfo? defaultCulture)
		{
			if (string.IsNullOrWhiteSpace(targetProject))
				throw new ArgumentNullException(nameof(targetProject));

			if (!targetProject.EndsWith("\\"))
			{
				targetProject += "\\";
			}

			var resFiles = ResourceLoader.LoadResources(targetProject, assembly, defaultCulture, ResourceLoadOptions.LoadResxAllResources, SearchOption.AllDirectories);

			ResourceKeysGenerator resourcesGenerator = new ResourceKeysGenerator
			{
				WriteMode = GeneratorBase.WriteModes.Overwritte
			};
			resourcesGenerator.SetParam("TargetProject", targetProject);
			resourcesGenerator.SetParam("RootNamespace", rootNamespace);
			resourcesGenerator.SetParam("ResFiles", resFiles);
			resourcesGenerator.TransformText();
			var errors = resourcesGenerator.ErrorString();
			if (!string.IsNullOrWhiteSpace(errors))
				throw new Exception(errors);
		}
	}
}
