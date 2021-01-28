using System.Reflection;

namespace Raider.Extensions
{
	public static class AssemblyExtensions
	{
		public static string GetPublicKeyToken(this Assembly assembly)
		{
			var bytes = assembly.GetName().GetPublicKeyToken();
			if (bytes == null || bytes.Length == 0)
				return "None";

			var publicKeyToken = string.Empty;
			for (int i = 0; i < bytes.GetLength(0); i++)
				publicKeyToken += string.Format("{0:x2}", bytes[i]);

			return publicKeyToken;
		}

		public static string GetPublicKey(this Assembly assembly)
		{
			var bytes = assembly.GetName().GetPublicKey();
			if (bytes == null || bytes.Length == 0)
				return "None";

			var publicKeyToken = string.Empty;
			for (int i = 0; i < bytes.GetLength(0); i++)
				publicKeyToken += string.Format("{0:x2}", bytes[i]);

			return publicKeyToken;
		}
	}
}
