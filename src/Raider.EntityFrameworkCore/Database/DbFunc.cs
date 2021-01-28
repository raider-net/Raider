using System;

namespace Raider.EntityFrameworkCore.Database
{
	public static class DbFunc
	{
		public static string Unaccent(string? text)
			=> throw new NotSupportedException($"Method {nameof(Unaccent)} can be called only in LINQ-to-Entities!");
	}
}
