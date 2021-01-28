namespace Raider.Extensions
{
	public static class NullableExtensions
	{
		public static T GetDefaultNullableValue<T>(this T? nullable)
			where T : struct
			=> default;
	}
}
