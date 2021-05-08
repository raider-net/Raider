//using Raider.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Threading.Tasks;

//namespace Raider.Reflection
//{
//    public sealed class ObjectWrapper
//    {
//        public TypeWrapper TypeWrapper { get; }
//		public object TargetInstance { get; private set; }
//		public ObjectWrapper BaseObjectWrapper { get; private set; }
//        public bool AllowMemberTypeCasting
//		{
//			get
//			{
//				return TypeWrapper.AllowMemberTypeCasting;
//			}
//			set
//			{
//				TypeWrapper.AllowMemberTypeCasting = value;
//			}
//		}
//		public Type Type => TypeWrapper.Type;

//		private ObjectWrapper(TypeWrapper typeWrapper, object innerObject, bool allowMemberTypeCasting)
//        {
//            this.TypeWrapper = typeWrapper;
//            this.TargetInstance = innerObject;
//            this.AllowMemberTypeCasting = allowMemberTypeCasting;
//        }

//		public ObjectWrapper SetTargetInstance(object instance)
//		{
//			if (instance == null)
//				TargetInstance = null;
//			else if (instance.GetType() == TypeWrapper.Type)
//				TargetInstance = instance;
//			else
//				throw new ArgumentException($"{instance} type {instance.GetType().FullName} must be type of {TypeWrapper.Type.FullName}");

//			return this;
//		}

//		//internal ObjectWrapper(TypeWrapper typeWrapper, ref object innerObject, bool allowMemberTypeCasting)
//		//{
//		//    this.typeWrapper = typeWrapper;
//		//    this.innerObject = innerObject;
//		//    this.AllowMemberTypeCasting = allowMemberTypeCasting;
//		//}

//		public static ObjectWrapper Create(Type objType, bool includeAllBaseTypes = false, List<System.Type> allowedBaseTypes = null, bool suppressExceptions = false, bool allowMemberTypeCasting = false)
//        {
//            return Create(objType, null, includeAllBaseTypes, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//        }

//        public static ObjectWrapper Create(Type objType, TypeWrapper tw, bool includeAllBaseTypes = false, List<System.Type> allowedBaseTypes = null, bool suppressExceptions = false, bool allowMemberTypeCasting = false)
//        {
//            if (!objType.IsClass && !objType.IsStruct())
//            {
//				throw new InvalidOperationException(objType.ToFriendlyFullName());
//            }
//            if (tw == null)
//            {
//                tw = TypeWrapper.Create(objType, suppressExceptions, allowMemberTypeCasting);
//            }
//            else
//            {
//                if (tw.Type != objType)
//                {
//					throw new Exception($"Type of ObjectWrapper {objType.ToFriendlyFullName()} is not equal to TypeWrapper type {tw.Type.ToFriendlyFullName()}");
//                }
//            }
//            ObjectWrapper result = new ObjectWrapper(tw, null, allowMemberTypeCasting);

//            System.Type baseObjType = objType.BaseType;
//            if (includeAllBaseTypes || (allowedBaseTypes != null && allowedBaseTypes.Contains(baseObjType)))
//            {
//                try
//                {
//                    if (baseObjType != null && baseObjType != typeof(object))
//                    {
//                        result.BaseObjectWrapper = ObjectWrapper.CreateBaseObject(objType, baseObjType, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//                    }
//                }
//                catch { }
//            }
//            return result;
//        }

//        private static ObjectWrapper CreateBaseObject(Type objType, System.Type baseType, List<System.Type> allowedBaseTypes, bool suppressExceptions, bool allowMemberTypeCasting)
//        {
//            if (!baseType.IsClass && !baseType.IsStruct())
//			{
//				throw new InvalidOperationException(objType.ToFriendlyFullName());
//			}
//            TypeWrapper tw = TypeWrapper.Create(baseType, suppressExceptions, allowMemberTypeCasting);
//            ObjectWrapper result = new ObjectWrapper(tw, null, allowMemberTypeCasting);

//            System.Type baseObjType = baseType.BaseType;
//            if (allowedBaseTypes == null || allowedBaseTypes.Contains(baseObjType))
//            {
//                try
//                {
//                    if (baseObjType != null && baseObjType != typeof(object))
//                    {
//                        result.BaseObjectWrapper = ObjectWrapper.CreateBaseObject(objType, baseObjType, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//                    }
//                }
//                catch { }
//            }
//            return result;
//        }

//        public static ObjectWrapper Create(object obj, bool includeAllBaseTypes = false, List<System.Type>? allowedBaseTypes = null, bool suppressExceptions = false, bool allowMemberTypeCasting = false)
//        {
//            return Create(obj, null, includeAllBaseTypes, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//        }

//        public static ObjectWrapper Create(object obj, TypeWrapper tw, bool includeAllBaseTypes = false, List<System.Type> allowedBaseTypes = null, bool suppressExceptions = false, bool allowMemberTypeCasting = false)
//        {
//			if (obj == null) throw new ArgumentNullException(nameof(obj));
//            System.Type objType = obj.GetType();
//            if (!objType.IsClass && !objType.IsStruct())
//			{
//				throw new InvalidOperationException(objType.ToFriendlyFullName());
//			}
//            if (tw == null)
//            {
//                tw = TypeWrapper.Create(objType, suppressExceptions, allowMemberTypeCasting);
//            }
//            else
//            {
//                if (tw.Type != objType)
//				{
//					throw new Exception($"Type of ObjectWrapper {objType.ToFriendlyFullName()} is not equal to TypeWrapper type {tw.Type.ToFriendlyFullName()}");
//				}
//            }
//            ObjectWrapper result = new ObjectWrapper(tw, obj, allowMemberTypeCasting);

//            System.Type baseObjType = objType.BaseType;
//            if (includeAllBaseTypes || (allowedBaseTypes != null && allowedBaseTypes.Contains(baseObjType)))
//            {
//                try
//                {
//                    if (baseObjType != null && baseObjType != typeof(object))
//                    {
//                        result.BaseObjectWrapper = ObjectWrapper.CreateBaseObject(obj, baseObjType, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//                    }
//                }
//                catch { }
//            }
//            return result;
//        }

//        private static ObjectWrapper CreateBaseObject(object obj, System.Type baseType, List<System.Type> allowedBaseTypes, bool suppressExceptions, bool allowMemberTypeCasting)
//        {
//			if (obj == null) throw new ArgumentNullException(nameof(obj));
//            if (!baseType.IsClass && !baseType.IsStruct())
//			{
//				throw new InvalidOperationException(baseType.ToFriendlyFullName());
//			}
//            TypeWrapper tw = TypeWrapper.Create(baseType, suppressExceptions, allowMemberTypeCasting);
//            ObjectWrapper result = new ObjectWrapper(tw, obj, allowMemberTypeCasting);

//            System.Type baseObjType = baseType.BaseType;
//            if (allowedBaseTypes == null || allowedBaseTypes.Contains(baseObjType))
//            {
//                try
//                {
//                    if (baseObjType != null && baseObjType != typeof(object))
//                    {
//                        result.BaseObjectWrapper = ObjectWrapper.CreateBaseObject(obj, baseObjType, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//                    }
//                }
//                catch { }
//            }
//            return result;
//        }
        
//        public static ObjectWrapper Create<T>(bool includeAllBaseTypes = false, List<System.Type> allowedBaseTypes = null, bool suppressExceptions = false, bool allowMemberTypeCasting = false)
//        {
//            return Create<T>(null, includeAllBaseTypes, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//        }

//        public static ObjectWrapper Create<T>(TypeWrapper tw, bool includeAllBaseTypes = false, List<System.Type> allowedBaseTypes = null, bool suppressExceptions = false, bool allowMemberTypeCasting = false)
//        {
//            Type objType = typeof(T);
//            if (!objType.IsClass && !objType.IsStruct())
//			{
//				throw new InvalidOperationException(objType.ToFriendlyFullName());
//			}
//            if (tw == null)
//            {
//                tw = TypeWrapper.Create(objType, suppressExceptions, allowMemberTypeCasting);
//            }
//            else
//            {
//                if (tw.Type != objType)
//				{
//					throw new Exception($"Type of ObjectWrapper {objType.ToFriendlyFullName()} is not equal to TypeWrapper type {tw.Type.ToFriendlyFullName()}");
//				}
//            }
//            ObjectWrapper result = new ObjectWrapper(tw, null, allowMemberTypeCasting);

//            System.Type baseObjType = objType.BaseType;
//            if (includeAllBaseTypes || (allowedBaseTypes != null && allowedBaseTypes.Contains(baseObjType)))
//            {
//                try
//                {
//                    if (baseObjType != null && baseObjType != typeof(object))
//                    {
//                        result.BaseObjectWrapper = ObjectWrapper.CreateBaseObject<T>(baseObjType, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//                    }
//                }
//                catch { }
//            }
//            return result;
//        }

//        private static ObjectWrapper CreateBaseObject<T>(System.Type baseType, List<System.Type> allowedBaseTypes, bool suppressExceptions, bool allowMemberTypeCasting)
//        {
//            if (!baseType.IsClass && !baseType.IsStruct())
//			{
//				throw new InvalidOperationException(baseType.ToFriendlyFullName());
//			}
//            TypeWrapper tw = TypeWrapper.Create(baseType, suppressExceptions, allowMemberTypeCasting);
//            ObjectWrapper result = new ObjectWrapper(tw, null, allowMemberTypeCasting);

//            System.Type baseObjType = baseType.BaseType;
//            if (allowedBaseTypes == null || allowedBaseTypes.Contains(baseObjType))
//            {
//                try
//                {
//                    if (baseObjType != null && baseObjType != typeof(object))
//                    {
//                        result.BaseObjectWrapper = ObjectWrapper.CreateBaseObject<T>(baseObjType, allowedBaseTypes, suppressExceptions, allowMemberTypeCasting);
//                    }
//                }
//                catch { }
//            }
//            return result;
//        }
		

//        //internal static ObjectWrapper CreateFromStruct(ref object obj)
//        //{
//        //    TypeWrapper tw = TypeWrapper.Create(obj.GetType());
//        //    return new ObjectWrapper(tw, ref obj);
//        //}

//        public object this[string name]
//        {
//            get
//            {
//                return TypeWrapper[TargetInstance, name];
//            }
//            set
//            {
//                TypeWrapper[TargetInstance, name] = value;
//            }
//		}

//		#region InvokeMethods

//		public void InvokeMethod(string methodName)
//			=> TypeWrapper.InvokeMethod(TargetInstance, methodName, (Type[])null, (object[])null);

//		public void InvokeMethod(string methodName, Type methodParameterType, object methodParameter)
//			=> TypeWrapper.InvokeMethod(TargetInstance, methodName, new Type[] { methodParameterType }, new object[] { methodParameter });

//		public object InvokeMethod(string methodName, Type[] methodParameterTypes, object[] methodParameters)
//			=> TypeWrapper.InvokeMethod(TargetInstance, methodName, methodParameterTypes, methodParameters);

//		public void InvokeGenericMethod(string methodName, Type[] methodGenericTypes)
//			=> TypeWrapper.InvokeGenericMethod(TargetInstance, methodName, methodGenericTypes, (Type[])null, (object[])null);

//		public void InvokeGenericMethod(string methodName, Type[] methodGenericTypes, Type methodParameterType, object methodParameter)
//			=> TypeWrapper.InvokeGenericMethod(TargetInstance, methodName, methodGenericTypes, new Type[] { methodParameterType }, new object[] { methodParameter });

//		public object InvokeGenericMethod(string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, object[] methodParameters)
//			=> TypeWrapper.InvokeGenericMethod(TargetInstance, methodName, methodGenericTypes, methodParameterTypes, methodParameters);

//		public async Task InvokeMethodAsync(string methodName)
//			=> await TypeWrapper.InvokeMethodAsync(TargetInstance, methodName, (Type[])null, (object[])null);

//		public async Task InvokeMethodAsync(string methodName, Type methodParameterType, object methodParameter)
//			=> await TypeWrapper.InvokeMethodAsync(TargetInstance, methodName, new Type[] { methodParameterType }, new object[] { methodParameter });

//		public async Task<object> InvokeMethodAsync(string methodName, Type[] methodParameterTypes, object[] methodParameters)
//			=> await TypeWrapper.InvokeMethodAsync(TargetInstance, methodName, methodParameterTypes, methodParameters);

//		public async Task InvokeGenericMethodAsync(string methodName, Type[] methodGenericTypes)
//			=> await TypeWrapper.InvokeGenericMethodAsync(TargetInstance, methodName, methodGenericTypes, (Type[])null, (object[])null);

//		public async Task InvokeGenericMethodAsync(string methodName, Type[] methodGenericTypes, Type methodParameterType, object methodParameter)
//			=> await TypeWrapper.InvokeGenericMethodAsync(TargetInstance, methodName, methodGenericTypes, new Type[] { methodParameterType }, new object[] { methodParameter });

//		public async Task<object> InvokeGenericMethodAsync(string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, object[] methodParameters)
//			=> await TypeWrapper.InvokeGenericMethodAsync(TargetInstance, methodName, methodGenericTypes, methodParameterTypes, methodParameters);

//		#endregion InvokeMethods

//		public Dictionary<string, Type> GetAllMemberTypes()
//			=> TypeWrapper.GetAllMemberTypes();

//		public List<string> GetAllMemberNames()
//			=> TypeWrapper.GetAllMemberNames();

//		public MemberInfo GetMember(string memberName)
//			=> TypeWrapper.GetMember(memberName);

//		public Type GetMemberType(string memberName)
//			=> TypeWrapper.GetMemberType(memberName);

//		public Dictionary<string, Type> GetAllPropertyTypes()
//			=> TypeWrapper.GetAllPropertyTypes();

//		public List<string> GetAllPropertyNames()
//			=> TypeWrapper.GetAllPropertyNames();

//		public List<PropertyInfo> GetAllPropertyInfos()
//			=> TypeWrapper.GetAllPropertyInfos();

//		public PropertyInfo GetPropertyInfo(string propertyName)
//			=> TypeWrapper.GetPropertyInfo(propertyName);

//		public Type GetPropertyType(string propertyName)
//			=> TypeWrapper.GetPropertyType(propertyName);

//		public Dictionary<string, Type> GetPublicProperties()
//			=> TypeWrapper.GetPublicPropertyTypes();

//		public Dictionary<string, Type> GetPublicPropertiesWithSetters()
//			=> TypeWrapper.GetPublicPropertyTypesWithSetters();

//		public Dictionary<string, Type> GetAllFieldTypes()
//			=> TypeWrapper.GetAllFieldTypes();

//		public List<string> GetAllFieldNames()
//			=> TypeWrapper.GetAllFieldNames();

//		public List<FieldInfo> GetAllFieldInfos()
//			=> TypeWrapper.GetAllFieldInfos();

//		public FieldInfo GetFieldInfo(string fieldName)
//			=> TypeWrapper.GetFieldInfo(fieldName);

//		public Type GetFieldType(string fieldName)
//			=> TypeWrapper.GetFieldType(fieldName);

//		public Dictionary<string, Type> GetPublicFieldTypes()
//			=> TypeWrapper.GetPublicFieldTypes();

//		public ConstructorInfo GetConstructorInfo(params Type[] paramTypes)
//			=> TypeWrapper.GetConstructorInfo(paramTypes);

//		public List<MethodInfo> GetAllMethodInfos()
//			=> TypeWrapper.GetAllMethodInfos();

//		public MethodInfo GetMethodInfo(string methodName, params Type[] paramTypes)
//			=> TypeWrapper.GetMethodInfo(methodName, paramTypes);

//		public MethodInfo GetGenericMethodInfo(string methodName, Type[] genericTypes, params Type[] paramTypes)
//			=> TypeWrapper.GetGenericMethodInfo(methodName, genericTypes, paramTypes);

//		public PropertyWrapper GetPropertyWrapper(string propertyName)
//			=> TypeWrapper.GetPropertyWrapper(propertyName);

//		public FieldWrapper GetFieldWrapper(string fieldName)
//			=> TypeWrapper.GetFieldWrapper(fieldName);

//		public ConstructorWrapper GetConstructorWrapper(params Type[] paramTypes)
//			=> TypeWrapper.GetConstructorWrapper(paramTypes);
		
//		public MethodWrapper GetMethodWrapper(string methodName, params Type[] paramTypes)
//			=> TypeWrapper.GetMethodWrapper(methodName, paramTypes);

//		public MethodWrapper GetGenericMethodWrapper(string methodName, Type[] genericTypes, Type[] paramTypes, out MethodInfo genericMethodInfo)
//			=> TypeWrapper.GetGenericMethodWrapper(methodName, genericTypes, paramTypes, out genericMethodInfo);
//	}
//}
