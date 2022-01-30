//#if NETSTANDARD2_0 || NETSTANDARD2_1
//using System;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;

//namespace Raider.Plugins.Internal
//{
//	internal class PathInternal
//	{
//		internal const char WINDOWS_DirectorySeparatorChar = '\\';
//		internal const char WINDOWS_AltDirectorySeparatorChar = '/';
//		internal const char WINDOWS_VolumeSeparatorChar = ':';

//		public const char UNIX_DirectorySeparatorChar = '/';

//		public static bool IsPathFullyQualified(string path)
//		{
//			if (path == null)
//				throw new ArgumentNullException(nameof(path));

//			return IsPathFullyQualified(path.AsSpan());
//		}

//		public static bool IsPathFullyQualified(ReadOnlySpan<char> path)
//		{
//			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//				return !WINDOWS_IsPartiallyQualified(path);
//			else
//				return !UNIX_IsPartiallyQualified(path);
//		}

//		public static bool UNIX_IsPartiallyQualified(ReadOnlySpan<char> path)
//		{
//			return !UNIX_IsPathRooted(path);
//		}

//		public static bool UNIX_IsPathRooted(ReadOnlySpan<char> path)
//		{
//			return path.Length > 0 && path[0] == UNIX_DirectorySeparatorChar;
//		}

//		public static bool WINDOWS_IsPartiallyQualified(ReadOnlySpan<char> path)
//		{
//			if (path.Length < 2)
//			{
//				// It isn't fixed, it must be relative.  There is no way to specify a fixed
//				// path with one character (or less).
//				return true;
//			}

//			if (WINDOWS_IsDirectorySeparator(path[0]))
//			{
//				// There is no valid way to specify a relative path with two initial slashes or
//				// \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
//				return !(path[1] == '?' || WINDOWS_IsDirectorySeparator(path[1]));
//			}

//			// The only way to specify a fixed path that doesn't begin with two slashes
//			// is the drive, colon, slash format- i.e. C:\
//			return !((path.Length >= 3)
//				&& (path[1] == WINDOWS_VolumeSeparatorChar)
//				&& WINDOWS_IsDirectorySeparator(path[2])
//				// To match old behavior we'll check the drive character for validity as the path is technically
//				// not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
//				&& WINDOWS_IsValidDriveChar(path[0]));
//		}

//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public static bool WINDOWS_IsDirectorySeparator(char c)
//		{
//			return c == WINDOWS_DirectorySeparatorChar || c == WINDOWS_AltDirectorySeparatorChar;
//		}

//		internal static bool WINDOWS_IsValidDriveChar(char value)
//		{
//			return (value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z');
//		}
//	}
//}
//#endif
