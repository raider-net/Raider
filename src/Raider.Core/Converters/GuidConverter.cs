using System;
using System.Linq;
using Raider.Extensions;

namespace Raider.Converters
{
	public static class GuidConverter
	{
		public static readonly Guid RAIDER_STRING_NAMESPACE = new("308D1EBE-6253-428B-9F10-8A80F8A7BCE5");

		public enum StringUuidVersionHashAlgorithm
		{
			MD5 = 3,
			SHA1 = 5
		}

		public static Guid ToGuid(sbyte value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static sbyte GuidToSByte(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 2)
				: value.ToByteArray();

			sbyte result = Convert.ToSByte(BitConverter.ToInt16(b, 0));
			return result;
		}

		public static Guid ToGuid(byte value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static byte GuidToByte(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 2)
				: value.ToByteArray();

			byte result = Convert.ToByte(BitConverter.ToUInt16(b, 0));
			return result;
		}

		public static Guid ToGuid(short value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static short GuidToInt16(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 2)
				: value.ToByteArray();

			short result = BitConverter.ToInt16(b, 0);
			return result;
		}

		public static Guid ToGuid(ushort value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static ushort GuidToUInt16(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 2)
				: value.ToByteArray();

			ushort result = BitConverter.ToUInt16(b, 0);
			return result;
		}

		public static Guid ToGuid(int value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static int GuidToInt32(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 4)
				: value.ToByteArray();

			int result = BitConverter.ToInt32(b, 0);
			return result;
		}

		public static Guid ToGuid(uint value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static uint GuidToUInt32(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 4)
				: value.ToByteArray();

			uint result = BitConverter.ToUInt32(b, 0);
			return result;
		}

		public static Guid ToGuid(long value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static long GuidToInt64(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 8)
				: value.ToByteArray();

			long result = BitConverter.ToInt64(b, 0);
			return result;
		}

		public static Guid ToGuid(ulong value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static ulong GuidToUInt64(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 8)
				: value.ToByteArray();

			ulong result = BitConverter.ToUInt64(b, 0);
			return result;
		}

		public static Guid ToGuid(float value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static float GuidToSingle(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 4)
				: value.ToByteArray();

			float result = BitConverter.ToSingle(b, 0);
			return result;
		}

		public static Guid ToGuid(double value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static double GuidToDouble(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 8)
				: value.ToByteArray();

			double result = BitConverter.ToDouble(b, 0);
			return result;
		}

		public static Guid ToGuid(decimal value)
		{
			byte[] bytes = decimal.GetBits(value).SelectMany(x => BitConverter.GetBytes(x)).ToArray();
			var result = new Guid(bytes);
			return result;
		}

		public static decimal GuidToDecimal(Guid value)
		{
			byte[] b = //trimGuid
				//? TrimGuidBytes(value, 16)
				//: 
				value.ToByteArray();

			var bits = new int[4];
			for (int i = 0; i < 4; i++)
			{
				bits[i] = BitConverter.ToInt32(b.Skip(i * 4).Take(4).ToArray(), 0);
			}

			var result = new decimal(bits);
			return result;
		}

		public static Guid ToGuid(char value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static char GuidToChar(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 2)
				: value.ToByteArray();

			char result = BitConverter.ToChar(b, 0);
			return result;
		}

		public static Guid ToGuid(bool value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static bool GuidToBoolean(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 1)
				: value.ToByteArray();

			bool result = BitConverter.ToBoolean(b, 0);
			return result;
		}

		public static Guid ToGuid(DateTime value)
		{
			byte[] bytes = new byte[16];
			BitConverter.GetBytes(value.Ticks).CopyTo(bytes, 0);
			var result = new Guid(bytes);
			return result;
		}

		public static DateTime GuidToDateTime(Guid value, bool trimGuid = true)
		{
			byte[] b = trimGuid
				? TrimGuidBytes(value, 8)
				: value.ToByteArray();

			var result = new DateTime(BitConverter.ToInt64(b, 0));
			return result;
		}

		/// <summary>
		/// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
		/// </summary>
		/// <param name="value">The value (within that namespace).</param>
		/// <returns>A UUID derived from the <see cref="RAIDER_STRING_NAMESPACE"/> namespace and value.</returns>
		public static Guid ToGuid(string value) =>
			ToGuid(value, RAIDER_STRING_NAMESPACE, StringUuidVersionHashAlgorithm.SHA1);

		/// <summary>
		/// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
		/// </summary>
		/// <param name="value">The value (within that namespace).</param>
		/// <param name="namespaceId">The ID of the namespace.</param>
		/// <param name="version">The version number of the UUID to create; this value must be either
		/// 3 (for MD5 hashing) or 5 (for SHA-1 hashing).</param>
		/// <returns>A UUID derived from the namespace and value.</returns>
		public static Guid ToGuid(string value, Guid namespaceId, StringUuidVersionHashAlgorithm version)
		{
			int versionNumber = (int)version;

			if (namespaceId == Guid.Empty)
				throw new ArgumentException("Namespace cannot be an empty GUID.", nameof(namespaceId));

			if (namespaceId == default)
				throw new ArgumentNullException(nameof(namespaceId), "Namespace cannot be null or empty.");

			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException(nameof(value), "Name cannot be null or empty.");

			if (versionNumber != 3 && versionNumber != 5)
				throw new ArgumentOutOfRangeException(nameof(versionNumber), "version must be either 3 or 5.");

			// convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
			// ASSUME: UTF-8 encoding is always appropriate
			var nameBytes = System.Text.Encoding.UTF8.GetBytes(value);

			if (nameBytes.Length == 0)
				throw new InvalidOperationException($"{nameof(nameBytes)}.Length == 0");

			// convert the namespace UUID to network order (step 3)
			var namespaceBytes = namespaceId.ToByteArray();
			SwapByteOrder(namespaceBytes);

			// compute the hash of the name space ID concatenated with the name (step 4)
			byte[] hash;
			using (var algorithm = versionNumber == 3
					? (System.Security.Cryptography.HashAlgorithm)System.Security.Cryptography.MD5.Create()
					: System.Security.Cryptography.SHA1.Create())
			{
				var combinedBytes = new byte[namespaceBytes.Length + nameBytes.Length];
				Buffer.BlockCopy(namespaceBytes, 0, combinedBytes, 0, namespaceBytes.Length);
				Buffer.BlockCopy(nameBytes, 0, combinedBytes, namespaceBytes.Length, nameBytes.Length);

				hash = algorithm.ComputeHash(combinedBytes);
			}

			// most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
			var newGuid = new byte[16];
			Array.Copy(hash, 0, newGuid, 0, 16);

			// set the four most significant bits (bits 12 through 15) of the time_hi_and_version
			// field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
			newGuid[6] = (byte)((newGuid[6] & 0x0F) | (versionNumber << 4));

			// set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved
			// to zero and one, respectively (step 10)
			newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

			// convert the resulting UUID to local byte order (step 13)
			SwapByteOrder(newGuid);
			return new Guid(newGuid);
		}

		// Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
		private static void SwapByteOrder(byte[] guid)
		{
			SwapBytes(guid, 0, 3);
			SwapBytes(guid, 1, 2);
			SwapBytes(guid, 4, 5);
			SwapBytes(guid, 6, 7);
		}

		private static void SwapBytes(byte[] guid, int left, int right)
		{
			var temp = guid[left];
			guid[left] = guid[right];
			guid[right] = temp;
		}

		public static Guid WriteToGuid(sbyte value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 2; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(byte value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 2; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(short value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 2; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(ushort value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 2; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(int value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 4; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(uint value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 4; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(long value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 8; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(ulong value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 8; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(float value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 4; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(double value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 8; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(decimal value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = decimal.GetBits(value).SelectMany(x => BitConverter.GetBytes(x)).ToArray();

			for (int i = 0; i < 16; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(char value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			for (int i = 0; i < 2; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(bool value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value);

			bytes[0] = b[0];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid WriteToGuid(DateTime value, Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			var b = BitConverter.GetBytes(value.Ticks);

			for (int i = 0; i < 8; i++)
				bytes[i] = b[i];

			var result = new Guid(bytes);
			return result;
		}

		public static Guid TrimGuidToType<T>(Guid value)
			=> TrimGuidToType(value, typeof(T));

		public static Guid TrimGuidToType(Guid value, Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			type = type.GetUnderlyingNullableType();

			var typeLength = 16;

			if (type == typeof(sbyte))
				typeLength = 2;
			if (type == typeof(byte))
				typeLength = 2;
			if (type == typeof(short))
				typeLength = 2;
			if (type == typeof(ushort))
				typeLength = 2;
			if (type == typeof(int))
				typeLength = 4;
			if (type == typeof(uint))
				typeLength = 4;
			if (type == typeof(long))
				typeLength = 8;
			if (type == typeof(ulong))
				typeLength = 8;
			if (type == typeof(float))
				typeLength = 4;
			if (type == typeof(double))
				typeLength = 8;
			if (type == typeof(decimal))
				typeLength = 16;
			if (type == typeof(char))
				typeLength = 2;
			if (type == typeof(bool))
				typeLength = 1;
			if (type == typeof(DateTime))
				typeLength = 8;

			var bytes = TrimGuidBytes(value, typeLength);
			var result = new Guid(bytes);
			return result;
		}

		private static byte[] TrimGuidBytes(Guid value, int typeLength)
		{
			byte[] bytes = value.ToByteArray();
			
			for (int i = typeLength; i < bytes.Length; i++)
				bytes[i] = 0;

			return bytes;
		}
	}
}
