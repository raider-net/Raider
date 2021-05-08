//using Raider.Extensions;
//using System;
//using System.Reflection;

//namespace Raider.Reflection
//{
//	public sealed class PropertyWrapper
//	{
//		private MemberGetter _getter;
//		private MemberSetter _setter;
//		private MemberSetter _strucSetter;

//		public PropertyInfo PropertyInfo { get; }
//		public string PropertyName => PropertyInfo.Name;
//		public MemberGetter Getter => GetGetter();
//		public MemberSetter Setter => GetSetter();
//		public MemberSetter StrucSetter => GetStructSetter();

//		public PropertyWrapper(PropertyInfo propertyInfo)
//		{
//			PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
//		}

//		private MemberGetter GetGetter()
//		{
//			if (_getter != null)
//				return _getter;

//			_getter = PropertyInfo.DelegateForGetPropertyValue();
//			return _getter;
//		}

//		private MemberSetter GetSetter()
//		{
//			if (_setter != null)
//				return _setter;

//			_setter = PropertyInfo.DelegateForSetPropertyValue();
//			return _setter;
//		}

//		private MemberSetter GetStructSetter()
//		{
//			if (_strucSetter != null)
//				return _strucSetter;

//			_strucSetter = PropertyInfo.DelegateForSetPropertyValueForStruct();
//			return _strucSetter;
//		}

//		public PropertyWrapper Initialize()
//		{
//			GetGetter();
//			GetSetter();
//			GetStructSetter();
//			return this;
//		}

//		public object Get(object target)
//		{
//			var t = (PropertyInfo.IsStaticProperty() ? null : target);
//			return Getter(t);
//		}

//		public void Set(object target, object value)
//		{
//			var t = (PropertyInfo.IsStaticProperty() ? null : target);

//			if (t != null && t.GetType().IsStruct())
//			{
//				StrucSetter(t, value);
//			}
//			else
//			{
//				Setter(t, value);
//			}
//		}
//	}
//}
