using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Raider.Enums
{
	[System.Diagnostics.DebuggerDisplay("Data: {Key} = {Value}")]
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
	public class DataAttributeAttribute : Attribute
	{
		public readonly string Key;
		public readonly object Value;

		public DataAttributeAttribute(string key, object value)
		{
			this.Key = key;
			this.Value = value;
		}
	}

	public static class EnumHelper
	{
		public static int ConvertEnumToInt(Enum en)
		{
			return Convert.ToInt32(en);
		}

		public static List<DataAttributeAttribute> GetAllDataAttributes(Enum en)
		{
			if (en == null)
			{
				throw new System.ArgumentNullException(nameof(en));
			}
			Type enumType = en.GetType();
			FieldInfo fi = enumType.GetField(en.ToString());
			return fi.GetCustomAttributes(typeof(DataAttributeAttribute), true).Cast<DataAttributeAttribute>().ToList();
		}

		public static string GetFullPathToEnumField(Enum enumField)
		{
			if (enumField == null)
			{
				throw new System.ArgumentNullException(nameof(enumField));
			}
			return string.Format("{0}.{1}", enumField.GetType().FullName, enumField.ToString());
		}

		public static object GetFirstDataAttributeValue(Enum en)
		{
			return GetDataAttributeValue(en, null);
		}

		public static object GetDataAttributeValue(Enum en, string dataKey)
		{
			if (en == null)
			{
				throw new System.ArgumentNullException(nameof(en));
			}
			Type enumType = en.GetType();
			FieldInfo fi = enumType.GetField(en.ToString());
			DataAttributeAttribute dataAttr = fi.GetCustomAttributes(typeof(DataAttributeAttribute), true).Cast<DataAttributeAttribute>().FirstOrDefault(x => dataKey == null || x.Key == dataKey);
			return dataAttr?.Value;
		}

		#region object

		public static List<object> GetAllEnumValues(Type enumType)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			return Enum.GetValues(enumType).OfType<object>().ToList();
		}

		public static List<string> GetAllEnumValuesAsStrings(Type enumType)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			List<object> enumValues = GetAllEnumValues(enumType);
			List<string> result = new List<string>();
			if (enumValues != null && 0 < enumValues.Count)
			{
				result.AddRange(enumValues.Select(x => x.ToString()));
			}
			return result;
		}

		public static List<int> GetAllEnumValuesAsInts(Type enumType)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			Enum[] enumValues = EnumHelper.GetAllEnumValues(enumType).Cast<Enum>().ToArray();
			return enumValues.Cast<int>().ToList();
		}

		public static object ConvertIntToEnum(Type enumType, int enumIntValue)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				return Enum.ToObject(enumType, enumIntValue);
			}
			else
			{
				throw new System.ArgumentException($"Requested int value {enumIntValue} was not found in enum type {enumType.ToFriendlyFullName()}");
			}
		}

		public static object ConvertIntToEnumWithDefault(Type enumType, int enumIntValue, Enum defaultValue)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				return Enum.ToObject(enumType, enumIntValue);
			}
			else
			{
				return defaultValue;
			}
		}

		public static object ConvertStringToEnum(Type enumType, string enumStringValue)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			object result = Enum.Parse(enumType, enumStringValue);
			return result;
		}

		public static object ConvertStringToEnum(Type enumType, string enumStringValue, bool ignoreCase)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			object result = Enum.Parse(enumType, enumStringValue, ignoreCase);
			return result;
		}

		public static object ConvertStringToEnumWithDefault(Type enumType, string enumStringValue, Enum defaultValue)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			try
			{
				object result = Enum.Parse(enumType, enumStringValue);
				return result;
			}
			catch
			{
				return defaultValue;
			}
		}

		public static object ConvertStringToEnumWithDefault(Type enumType, string enumStringValue, Enum defaultValue, bool ignoreCase)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			try
			{
				object result = Enum.Parse(enumType, enumStringValue, ignoreCase);
				return result;
			}
			catch
			{
				return defaultValue;
			}
		}

		public static int ConvertStringToInt(Type enumType, string enumStringValue)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			Enum e = (Enum)EnumHelper.ConvertStringToEnum(enumType, enumStringValue);
			return Convert.ToInt32(e);
		}

		public static int ConvertStringToInt(Type enumType, string enumStringValue, bool ignoreCase)
		{
			if (enumType == null)
			{
				throw new System.ArgumentNullException(nameof(enumType));
			}
			Enum e = (Enum)EnumHelper.ConvertStringToEnum(enumType, enumStringValue, ignoreCase);
			return Convert.ToInt32(e);
		}

		public static object ConvertIntToDataAttributeValue(Type enumType, int enumIntValue)
		{
			return ConvertIntToDataAttributeValue(enumType, enumIntValue, (string)null);
		}

		public static object ConvertIntToDataAttributeValue(Type enumType, int enumIntValue, string dataKey)
		{
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				Enum en = (Enum)Enum.ToObject(enumType, enumIntValue);
				return GetDataAttributeValue(en, dataKey);
			}
			else
			{
				throw new System.ArgumentException($"Requested int value {enumIntValue} was not found in enum type {enumType.ToFriendlyFullName()}");
			}
		}

		public static object ConvertIntToDataAttributeValueWithDefault(Type enumType, int enumIntValue, Enum defaultEnumValue)
		{
			return ConvertIntToDataAttributeValueWithDefault(enumType, enumIntValue, null, defaultEnumValue);
		}

		public static object ConvertIntToDataAttributeValueWithDefault(Type enumType, int enumIntValue, string dataKey, Enum defaultEnumValue)
		{
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				Enum en = (Enum)Enum.ToObject(enumType, enumIntValue);
				return GetDataAttributeValue(en, dataKey);
			}
			else
			{
				return GetDataAttributeValue(defaultEnumValue, dataKey);
			}
		}

		public static object ConvertIntToDataAttributeValueWithDefault(Type enumType, int enumIntValue, object defaultValue)
		{
			return ConvertIntToDataAttributeValueWithDefault(enumType, enumIntValue, null, defaultValue);
		}

		public static object ConvertIntToDataAttributeValueWithDefault(Type enumType, int enumIntValue, string dataKey, object defaultValue)
		{
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				Enum en = (Enum)Enum.ToObject(enumType, enumIntValue);
				return GetDataAttributeValue(en, dataKey);
			}
			else
			{
				return defaultValue;
			}
		}

		public static object ConvertStringToDataAttributeValue(Type enumType, string enumStringValue)
		{
			return ConvertStringToDataAttributeValue(enumType, enumStringValue, (string)null);
		}

		public static object ConvertStringToDataAttributeValue(Type enumType, string enumStringValue, string dataKey)
		{
			Enum result = (Enum)Enum.Parse(enumType, enumStringValue);
			if (result != null)
			{
				return GetDataAttributeValue(result, dataKey);
			}
			else
			{
				throw new System.ArgumentException($"Requested string enum value {enumStringValue} was not found in enum type {enumType.ToFriendlyFullName()}");
			}
		}

		public static object ConvertStringToDataAttributeValueWithDefault(Type enumType, string enumStringValue, Enum defaultEnumValue)
		{
			return ConvertStringToDataAttributeValueWithDefault(enumType, enumStringValue, null, defaultEnumValue);
		}

		public static object ConvertStringToDataAttributeValueWithDefault(Type enumType, string enumStringValue, string dataKey, Enum defaultEnumValue)
		{
			Enum result = (Enum)Enum.Parse(enumType, enumStringValue);
			if (result != null)
			{
				return GetDataAttributeValue(result, dataKey);
			}
			else
			{
				return GetDataAttributeValue(defaultEnumValue as Enum, dataKey);
			}
		}

		public static object ConvertStringToDataAttributeValueWithDefault(Type enumType, string enumStringValue, object defaultValue)
		{
			return ConvertStringToDataAttributeValueWithDefault(enumType, enumStringValue, null, defaultValue);
		}

		public static object ConvertStringToDataAttributeValueWithDefault(Type enumType, string enumStringValue, string dataKey, object defaultValue)
		{
			Enum result = (Enum)Enum.Parse(enumType, enumStringValue);
			if (result != null)
			{
				return GetDataAttributeValue(result, dataKey);
			}
			else
			{
				return defaultValue;
			}
		}

		public static Enum ConvertDataAttributeValueToEnum(Type enumType, string dataValue)
		{
			return ConvertDataAttributeValueToEnum(enumType, null, dataValue);
		}

		private static DataAttributeAttribute GetDataAttribute(FieldInfo fi, string dataKey, object dataValue)
		{
			DataAttributeAttribute da = fi.GetCustomAttributes(typeof(DataAttributeAttribute), true).Cast<DataAttributeAttribute>().FirstOrDefault(x => (dataKey == null || x.Key == dataKey) && x.Value == dataValue);
			return da;
		}

		public static Enum ConvertDataAttributeValueToEnum(Type enumType, string dataKey, string dataValue)
		{
			foreach (Enum enValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enValue.ToString());
				DataAttributeAttribute da = GetDataAttribute(fi, dataKey, dataValue);
				if (da != null)
				{
					return enValue;
				}
			}
			throw new System.ArgumentException($"Requested DataAttribute(\"{dataKey}\", \"{dataValue}\") was not found in enum type {enumType.ToFriendlyFullName()}");
		}

		public static Enum ConvertDataAttributeValueToEnumWithDefault(Type enumType, string dataValue, Enum defaultEnumValue)
		{
			return ConvertDataAttributeValueToEnumWithDefault(enumType, null, dataValue, defaultEnumValue);
		}

		public static Enum ConvertDataAttributeValueToEnumWithDefault(Type enumType, string dataKey, string dataValue, Enum defaultEnumValue)
		{
			foreach (Enum enValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enValue.ToString());
				DataAttributeAttribute da = GetDataAttribute(fi, dataKey, dataValue);
				if (da != null)
				{
					return enValue;
				}
			}
			return defaultEnumValue;
		}

		public static int ConvertDataAttributeValueToInt(Type enumType, string dataValue)
		{
			return ConvertDataAttributeValueToInt(enumType, null, dataValue);
		}

		public static int ConvertDataAttributeValueToInt(Type enumType, string dataKey, string dataValue)
		{
			foreach (Enum enValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enValue.ToString());
				DataAttributeAttribute da = GetDataAttribute(fi, dataKey, dataValue);
				if (da != null)
				{
					return (int)(object)enValue;
				}
			}
			throw new System.ArgumentException($"Requested DataAttribute(\"{dataKey}\", \"{dataValue}\") was not found in enum type {enumType.ToFriendlyFullName()}");
		}

		public static int ConvertDataAttributeValueToIntWithDefault(Type enumType, string dataValue, int defaultEnumValue)
		{
			return ConvertDataAttributeValueToIntWithDefault(enumType, null, dataValue, defaultEnumValue);
		}

		public static int ConvertDataAttributeValueToIntWithDefault(Type enumType, string dataKey, string dataValue, int defaultEnumValue)
		{
			foreach (Enum enValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enValue.ToString());
				DataAttributeAttribute da = GetDataAttribute(fi, dataKey, dataValue);
				if (da != null)
				{
					return (int)(object)enValue;
				}
			}
			return defaultEnumValue;
		}

		#endregion object

		#region TEnum

		public static List<TEnum> GetAllEnumValues<TEnum>() where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
		}

		public static Dictionary<TEnum, IEnumerable<object>> GetAllEnumDataValues<TEnum>(string dataKey) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return Enum.GetValues(typeof(TEnum))
						.Cast<TEnum>()
						.ToDictionary(
							x => x,
							y => GetAllDataAttributes(y as Enum)
									.Where(x => x.Key == dataKey)
									.Select(x => x.Value));
		}

		public static Dictionary<TEnum, object> GetAllEnumFirstDataValues<TEnum>(string dataKey) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToDictionary(x => x, y => GetDataAttributeValue(y as Enum, dataKey));
		}

		public static List<string> GetAllEnumValuesAsStrings<TEnum>() where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			List<TEnum> enumValues = GetAllEnumValues<TEnum>();
			List<string> result = new List<string>();
			if (enumValues != null && 0 < enumValues.Count)
			{
				result.AddRange(enumValues.Select(x => x.ToString()));
			}
			return result;
		}

		public static List<int> GetAllEnumValuesAsInts<TEnum>() where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Enum[] enumValues = EnumHelper.GetAllEnumValues<TEnum>().Cast<Enum>().ToArray();
			return enumValues.Cast<int>().ToList();
		}

		public static TEnum ConvertIntToEnum<TEnum>(int enumIntValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				return (TEnum)Enum.ToObject(enumType, enumIntValue);
			}
			else
			{
				throw new System.ArgumentException($"Requested int value {enumIntValue} was not found in enum type {enumType.ToFriendlyFullName()}");
			}
		}

		public static bool TryConvertIntToEnum<TEnum>(int enumIntValue, out TEnum result) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				result = (TEnum)Enum.ToObject(enumType, enumIntValue);
				return true;
			}
			else
			{
				result = default;
				return false;
			}
		}

		public static TEnum ConvertIntToEnumWithDefault<TEnum>(int enumIntValue, TEnum defaultValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				return (TEnum)Enum.ToObject(enumType, enumIntValue);
			}
			else
			{
				return defaultValue;
			}
		}

		public static TEnum ConvertStringToEnum<TEnum>(string enumStringValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (Enum.TryParse(enumStringValue, out TEnum result))
			{
				return result;
			}
			else
			{
				throw new System.ArgumentException($"Requested string enum value {enumStringValue} was not found in enum type {typeof(TEnum).ToFriendlyFullName()}");
			}
		}

		public static bool TryConvertStringToEnum<TEnum>(string enumStringValue, out TEnum result) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (Enum.TryParse(enumStringValue, out result))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static TEnum ConvertStringToEnum<TEnum>(string enumStringValue, bool ignoreCase) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (Enum.TryParse(enumStringValue, ignoreCase, out TEnum result))
			{
				return result;
			}
			else
			{
				throw new System.ArgumentException($"Requested string enum value {enumStringValue} was not found in enum type {typeof(TEnum).ToFriendlyFullName()}");
			}
		}

		public static bool TryConvertStringToEnum<TEnum>(string enumStringValue, bool ignoreCase, out TEnum result) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (Enum.TryParse(enumStringValue, ignoreCase, out result))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static TEnum ConvertStringToEnumWithDefault<TEnum>(string enumStringValue, TEnum defaultValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return Enum.TryParse<TEnum>(enumStringValue, out TEnum result) ? result : defaultValue;
		}

		public static TEnum ConvertStringToEnumWithDefault<TEnum>(string enumStringValue, TEnum defaultValue, bool ignoreCase) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return Enum.TryParse<TEnum>(enumStringValue, ignoreCase, out TEnum result) ? result : defaultValue;
		}

		public static int ConvertStringToInt<TEnum>(string enumStringValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			TEnum e = EnumHelper.ConvertStringToEnum<TEnum>(enumStringValue);
			return Convert.ToInt32(e);
		}

		public static bool TryConvertStringToInt<TEnum>(string enumStringValue, out int result) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (EnumHelper.TryConvertStringToEnum<TEnum>(enumStringValue, out TEnum e))
			{
				result = Convert.ToInt32(e);
				return true;
			}

			result = 0;
			return false;
		}

		public static int ConvertStringToInt<TEnum>(string enumStringValue, bool ignoreCase) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			TEnum e = EnumHelper.ConvertStringToEnum<TEnum>(enumStringValue, ignoreCase);
			return Convert.ToInt32(e);
		}

		public static bool TryConvertStringToInt<TEnum>(string enumStringValue, bool ignoreCase, out int result) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (EnumHelper.TryConvertStringToEnum<TEnum>(enumStringValue, ignoreCase, out TEnum e))
			{
				result = Convert.ToInt32(e);
				return true;
			}

			result = 0;
			return false;
		}

		public static object ConvertIntToDataAttributeValue<TEnum>(int enumIntValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertIntToDataAttributeValue<TEnum>(enumIntValue, null);
		}

		public static object ConvertIntToDataAttributeValue<TEnum>(int enumIntValue, string dataKey) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				Enum en = (Enum)Enum.ToObject(enumType, enumIntValue);
				return GetDataAttributeValue(en, dataKey);
			}
			else
			{
				throw new System.ArgumentException($"Requested int value {enumIntValue} was not found in enum type {enumType.ToFriendlyFullName()}");
			}
		}

		public static object ConvertIntToDataAttributeValueWithDefault<TEnum>(int enumIntValue, TEnum defaultEnumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertIntToDataAttributeValueWithDefault<TEnum>(enumIntValue, null, defaultEnumValue);
		}

		public static object ConvertIntToDataAttributeValueWithDefault<TEnum>(int enumIntValue, string dataKey, TEnum defaultEnumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				Enum en = (Enum)Enum.ToObject(enumType, enumIntValue);
				return GetDataAttributeValue(en, dataKey);
			}
			else
			{
				return GetDataAttributeValue(defaultEnumValue as Enum, dataKey);
			}
		}

		public static object ConvertIntToDataAttributeValueWithDefault<TEnum>(int enumIntValue, object defaultValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertIntToDataAttributeValueWithDefault<TEnum>(enumIntValue, null, defaultValue);
		}

		public static object ConvertIntToDataAttributeValueWithDefault<TEnum>(int enumIntValue, string dataKey, object defaultValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			if (Enum.IsDefined(enumType, enumIntValue))
			{
				Enum en = (Enum)Enum.ToObject(enumType, enumIntValue);
				return GetDataAttributeValue(en, dataKey);
			}
			else
			{
				return defaultValue;
			}
		}

		public static object ConvertStringToDataAttributeValue<TEnum>(string enumStringValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertStringToDataAttributeValue<TEnum>(enumStringValue, null);
		}

		public static object ConvertStringToDataAttributeValue<TEnum>(string enumStringValue, string dataKey) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (Enum.TryParse(enumStringValue, out TEnum result))
			{
				return GetDataAttributeValue(result as Enum, dataKey);
			}
			else
			{
				throw new System.ArgumentException($"Requested string enum value {enumStringValue} was not found in enum type {typeof(TEnum).ToFriendlyFullName()}");
			}
		}

		public static object ConvertStringToDataAttributeValueWithDefault<TEnum>(string enumStringValue, TEnum defaultEnumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertStringToDataAttributeValueWithDefault<TEnum>(enumStringValue, null, defaultEnumValue);
		}

		public static object ConvertStringToDataAttributeValueWithDefault<TEnum>(string enumStringValue, string dataKey, TEnum defaultEnumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (Enum.TryParse(enumStringValue, out TEnum result))
			{
				return GetDataAttributeValue(result as Enum, dataKey);
			}
			else
			{
				return GetDataAttributeValue(defaultEnumValue as Enum, dataKey);
			}
		}

		public static object ConvertStringToDataAttributeValueWithDefault<TEnum>(string enumStringValue, object defaultValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertStringToDataAttributeValueWithDefault<TEnum>(enumStringValue, null, defaultValue);
		}

		public static object ConvertStringToDataAttributeValueWithDefault<TEnum>(string enumStringValue, string dataKey, object defaultValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (Enum.TryParse(enumStringValue, out TEnum result))
			{
				return GetDataAttributeValue(result as Enum, dataKey);
			}
			else
			{
				return defaultValue;
			}
		}

		public static TEnum ConvertDataAttributeValueToEnum<TEnum>(string dataValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertDataAttributeValueToEnum<TEnum>(null, dataValue);
		}

		public static TEnum ConvertDataAttributeValueToEnum<TEnum>(string dataKey, string dataValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			foreach (Enum enValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enValue.ToString());
				DataAttributeAttribute da = GetDataAttribute(fi, dataKey, dataValue);
				if (da != null)
				{
					return (TEnum)(object)enValue;
				}
			}
			throw new System.ArgumentException($"Requested DataAttribute(\"{dataKey}\", \"{dataValue}\") was not found in enum type {enumType.ToFriendlyFullName()}");
		}

		public static TEnum ConvertDataAttributeValueToEnumWithDefault<TEnum>(string dataValue, TEnum defaultEnumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertDataAttributeValueToEnumWithDefault<TEnum>(null, dataValue, defaultEnumValue);
		}

		public static TEnum ConvertDataAttributeValueToEnumWithDefault<TEnum>(string dataKey, string dataValue, TEnum defaultEnumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			foreach (Enum enValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enValue.ToString());
				DataAttributeAttribute da = GetDataAttribute(fi, dataKey, dataValue);
				if (da != null)
				{
					return (TEnum)(object)enValue;
				}
			}
			return defaultEnumValue;
		}

		public static int ConvertDataAttributeValueToInt<TEnum>(string dataValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertDataAttributeValueToInt<TEnum>(null, dataValue);
		}

		public static int ConvertDataAttributeValueToInt<TEnum>(string dataKey, string dataValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			foreach (Enum enValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enValue.ToString());
				DataAttributeAttribute da = GetDataAttribute(fi, dataKey, dataValue);
				if (da != null)
				{
					return (int)(object)enValue;
				}
			}
			throw new System.ArgumentException($"Requested DataAttribute(\"{dataKey}\", \"{dataValue}\") was not found in enum type {enumType.ToFriendlyFullName()}");
		}

		public static int ConvertDataAttributeValueToIntWithDefault<TEnum>(string dataValue, int defaultEnumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			return ConvertDataAttributeValueToIntWithDefault<TEnum>(null, dataValue, defaultEnumValue);
		}

		public static int ConvertDataAttributeValueToIntWithDefault<TEnum>(string dataKey, string dataValue, int defaultEnumValue) where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Type enumType = typeof(TEnum);
			foreach (Enum enValue in Enum.GetValues(enumType))
			{
				FieldInfo fi = enumType.GetField(enValue.ToString());
				DataAttributeAttribute da = GetDataAttribute(fi, dataKey, dataValue);
				if (da != null)
				{
					return (int)(object)enValue;
				}
			}
			return defaultEnumValue;
		}

		#endregion TEnum
	}
}
