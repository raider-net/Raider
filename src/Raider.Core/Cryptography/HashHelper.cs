using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Raider.Cryptography
{
	public static class HashHelper
	{
		public static string ComputeSha256Hash(Stream data, bool seekStream = false)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			if (seekStream && data.CanSeek)
				data.Seek(0, SeekOrigin.Begin);

			using var sha256Hash = SHA256.Create();

			byte[] bytes = sha256Hash.ComputeHash(data);
			var sb = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
				sb.Append(bytes[i].ToString("x2"));

			if (seekStream && data.CanSeek)
				data.Seek(0, SeekOrigin.Begin);

			return sb.ToString();
		}

		public static string ComputeSha256Hash(string data)
			=> ComputeSha256Hash(Encoding.UTF8.GetBytes(data));

		public static string ComputeSha256Hash(byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			using var sha256Hash = SHA256.Create();

			byte[] bytes = sha256Hash.ComputeHash(data);
			var sb = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
				sb.Append(bytes[i].ToString("x2"));

			return sb.ToString();
		}
	}
}
