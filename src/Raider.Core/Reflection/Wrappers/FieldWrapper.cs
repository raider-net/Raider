using Raider.Extensions;
using System;
using System.Reflection;

namespace Raider.Reflection
{
	public sealed class FieldWrapper
	{
		private MemberGetter _getter;
		private MemberSetter _setter;
		private MemberSetter _strucSetter;

		public FieldInfo FieldInfo { get; }
		public string FieldName => FieldInfo.Name;
		public MemberGetter Getter => GetGetter();
		public MemberSetter Setter => GetSetter();
		public MemberSetter StrucSetter => GetStructSetter();

		public FieldWrapper(FieldInfo fieldInfo)
		{
			FieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
		}

		private MemberGetter GetGetter()
		{
			if (_getter != null)
				return _getter;

			_getter = FieldInfo.DelegateForGetFieldValue();
			return _getter;
		}

		private MemberSetter GetSetter()
		{
			if (_setter != null)
				return _setter;

			_setter = FieldInfo.DelegateForSetFieldValue();
			return _setter;
		}

		private MemberSetter GetStructSetter()
		{
			if (_strucSetter != null)
				return _strucSetter;

			_strucSetter = FieldInfo.DelegateForSetFieldValueForStruct();
			return _strucSetter;
		}

		public FieldWrapper Initialize()
		{
			GetGetter();
			GetSetter();
			GetStructSetter();
			return this;
		}

		public object Get(object target)
		{
			var t = (FieldInfo.IsStatic ? null : target);
			return Getter(t);
		}

		public void Set(object target, object value)
		{
			var t = (FieldInfo.IsStatic ? null : target);

			if (t != null && t.GetType().IsStruct())
			{
				StrucSetter(t, value);
			}
			else
			{
				Setter(t, value);
			}
		}
	}
}
