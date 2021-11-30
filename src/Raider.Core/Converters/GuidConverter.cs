using System;
using System.Linq;
using Raider.Extensions;

namespace Raider.Converters
{
	public static class GuidConverter
	{
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

		public static decimal GuidToDecimal(Guid value, bool trimGuid = true)
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
