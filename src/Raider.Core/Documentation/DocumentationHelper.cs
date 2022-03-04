//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace Raider.Documentation
//{
//	public static class DocumentationHelper
//	{
//		internal static HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();

//		public static string GetDirectoryPath(this Assembly assembly)
//		{
//			string codeBase = assembly.CodeBase;
//			UriBuilder uri = new UriBuilder(codeBase);
//			string path = Uri.UnescapeDataString(uri.Path);
//			return Path.GetDirectoryName(path);
//		}

//		internal static void LoadXmlDocumentation(Assembly assembly)
//		{
//			if (loadedAssemblies.Contains(assembly))
//			{
//				return; // Already loaded
//			}
//			string directoryPath = assembly.GetDirectoryPath();
//			string xmlFilePath = Path.Combine(directoryPath, assembly.GetName().Name + ".xml");
//			if (File.Exists(xmlFilePath))
//			{
//				LoadXmlDocumentation(File.ReadAllText(xmlFilePath));
//				loadedAssemblies.Add(assembly);
//			}
//		}
//	}
//}
