//#if NETSTANDARD2_0 || NETSTANDARD2_1
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Raider.Plugins.Internal
//{
//	internal partial struct LibraryNameVariation
//	{
//		private const string UNIX_LibraryNamePrefix = "lib";
//#if TARGET_OSX || TARGET_MACCATALYST || TARGET_IOS || TARGET_TVOS
//        private const string UNIX_LibraryNameSuffix = ".dylib";
//#else
//		private const string UNIX_LibraryNameSuffix = ".so";
//#endif

//		internal static IEnumerable<LibraryNameVariation> UNIX_DetermineLibraryNameVariations(string libName, bool isRelativePath, bool forOSLoader = false)
//		{
//			// This is a copy of the logic in DetermineLibNameVariations in dllimport.cpp in CoreCLR

//			if (!isRelativePath)
//			{
//				yield return new LibraryNameVariation(string.Empty, string.Empty);
//			}
//			else
//			{
//				bool containsSuffix = false;
//				int indexOfSuffix = libName.IndexOf(UNIX_LibraryNameSuffix, StringComparison.OrdinalIgnoreCase);
//				if (indexOfSuffix >= 0)
//				{
//					indexOfSuffix += UNIX_LibraryNameSuffix.Length;
//					containsSuffix = indexOfSuffix == libName.Length || libName[indexOfSuffix] == '.';
//				}

//				bool containsDelim = libName.Contains(PathInternal.UNIX_DirectorySeparatorChar);

//				if (containsSuffix)
//				{
//					yield return new LibraryNameVariation(string.Empty, string.Empty);
//					if (!containsDelim)
//					{
//						yield return new LibraryNameVariation(UNIX_LibraryNamePrefix, string.Empty);
//					}
//					yield return new LibraryNameVariation(string.Empty, UNIX_LibraryNameSuffix);
//					if (!containsDelim)
//					{
//						yield return new LibraryNameVariation(UNIX_LibraryNamePrefix, UNIX_LibraryNameSuffix);
//					}
//				}
//				else
//				{
//					yield return new LibraryNameVariation(string.Empty, UNIX_LibraryNameSuffix);
//					if (!containsDelim)
//					{
//						yield return new LibraryNameVariation(UNIX_LibraryNamePrefix, UNIX_LibraryNameSuffix);
//					}
//					yield return new LibraryNameVariation(string.Empty, string.Empty);
//					if (!containsDelim)
//					{
//						yield return new LibraryNameVariation(UNIX_LibraryNamePrefix, string.Empty);
//					}
//				}
//			}
//		}
//	}
//}
//#endif
