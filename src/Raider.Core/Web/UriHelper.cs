using System;

namespace Raider.Web
{
	public static class UriHelper
	{
		public static readonly char PATH_DELIMITER = '/';

		public static string? Combine(params string?[] paths)
		{
			if (paths == null) return null;
			if (paths.Length == 0) return "";
			if (paths.Length == 1) return paths[0];
			var finalPath = paths[0];

			for (int i = 1; i < paths.Length; i++)
			{
				finalPath = CombineInternal(finalPath, paths[i]);
			}

			return finalPath;
		}

		private static string? CombineInternal(string? path1, string? path2)
		{
			if (path1 == null || path1.Length == 0)
				return path2;

			if (path2 == null || path2.Length == 0)
				return path1;

			path1 = path1.TrimEnd('/', '\\').Replace("\\", "/");
			path2 = path2.TrimStart('/', '\\').Replace("\\", "/");

			return $"{path1}/{path2}";
		}

		public static string? GetBaseAUri(Uri uri)
		{
			if (uri == null) return null;
			return $"{uri.Scheme}://{uri.Authority}";
		}
	}
}
