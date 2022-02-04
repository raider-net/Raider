using Raider.Streams;
using System.IO;

namespace Raider.Extensions
{
	public static class StreamExtensions
	{
		public static void BlockCopy(this Stream source, Stream target, int blockSize = 65536)
		{
			int read;
			byte[] buffer = new byte[blockSize];
			while ((read = source.Read(buffer, 0, blockSize)) > 0)
			{
				target.Write(buffer, 0, read);
			}
		}

		public static byte[] ToArray(this Stream stream)
		{
			return StreamHelper.ToArray(stream);
		}
	}
}
