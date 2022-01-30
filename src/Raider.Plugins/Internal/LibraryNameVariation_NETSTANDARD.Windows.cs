//#if NETSTANDARD2_0 || NETSTANDARD2_1
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Raider.Plugins.Internal
//{
//	internal partial struct LibraryNameVariation
//	{
//		private const string WINDOWS_LibraryNameSuffix = ".dll";

//		internal static IEnumerable<LibraryNameVariation> WINDOWS_DetermineLibraryNameVariations(string libName, bool isRelativePath, bool forOSLoader = false)
//		{
//			// This is a copy of the logic in DetermineLibNameVariations in dllimport.cpp in CoreCLR

//			yield return new LibraryNameVariation(string.Empty, string.Empty);

//			// Follow LoadLibrary rules if forOSLoader is true
//			if (isRelativePath &&
//				(!forOSLoader || libName.Contains('.') && !libName.EndsWith(".")) &&
//				!libName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
//				!libName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
//			{
//				yield return new LibraryNameVariation(string.Empty, WINDOWS_LibraryNameSuffix);
//			}
//		}
//	}
//}
//#endif
