using Raider.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Raider.Reflection
{
	public sealed class TypeWrapper
	{
		private const string CTOR_NAME = ".ctor";

		private enum SuppressMessage
		{
			Unknown,
			OutOfGange,
			InvalidRead,
			InvalidWrite
		}

		private static readonly Hashtable WRAPPERS = new Hashtable();
		public Type Type { get; }
		private readonly Dictionary<string, FieldWrapper> _fieldsMap;
		private readonly Dictionary<string, PropertyWrapper> _propertiesMap;
		private readonly Dictionary<MethodKey, ConstructorWrapper> _ctorsMap;
		private readonly Dictionary<MethodKey, MethodWrapper> _methodsMap;
		private readonly List<MethodWrapper> _genericMethods;
		private readonly bool _suppressExceptions;

        public bool AllowMemberTypeCasting { get; set; } = false;

        private TypeWrapper(
			Dictionary<string, FieldWrapper> fieldsMap,
			Dictionary<string, PropertyWrapper> propertiesMap,
			Dictionary<MethodKey, ConstructorWrapper> ctorsMap,
			Dictionary<MethodKey, MethodWrapper> methodsMap,
			List<MethodWrapper> genericMethods,
			Type type,
			bool suppressExceptions,
			bool allowMemberTypeCasting)
        {
            this._fieldsMap = fieldsMap;
			this._propertiesMap = propertiesMap;
			this._ctorsMap = ctorsMap;
			this._methodsMap = methodsMap;
			this._genericMethods = genericMethods;
			this.Type = type;
            this._suppressExceptions = suppressExceptions;
            this.AllowMemberTypeCasting = allowMemberTypeCasting;
        }

		public static TypeWrapper Create(Type type)
			=> Create(type, false, false);

		public static TypeWrapper Create(Type type, bool suppressExceptions, bool allowMemberTypeCasting)
        {
			if (type == null) throw new ArgumentNullException(nameof(type));
            var lookup = WRAPPERS;
            TypeWrapper tw = (TypeWrapper)lookup[type];
            if (tw != null) return tw;

            lock (lookup)
            {
                // double-check
                tw = (TypeWrapper)lookup[type];
                if (tw != null) return tw;

                tw = CreateNew(type, suppressExceptions, allowMemberTypeCasting);

                lookup[type] = tw;
                return tw;
            }
        }

        private static TypeWrapper CreateNew(Type type, bool suppressExceptions, bool allowMemberTypeCasting)
        {
            PropertyInfo[] props = type.GetProperties(Flags.AllMembers);
            FieldInfo[] fields = type.GetFields(Flags.AllMembers);
			MethodInfo[] methods = type.GetMethods(Flags.AllMembers);
			ConstructorInfo[] ctors = type.GetConstructors(Flags.AllMembers);
			MethodInfo[] genMethods = methods.Where(m => m.IsGenericMethodDefinition).ToArray();
			methods = methods.Where(m => !m.IsGenericMethodDefinition).ToArray();

			var fieldsMap = new Dictionary<string, FieldWrapper>();
			var propertiesMap = new Dictionary<string, PropertyWrapper>();
			var ctorsMap = new Dictionary<MethodKey, ConstructorWrapper>();
			var methodsMap = new Dictionary<MethodKey, MethodWrapper>();
			var genericMethods = new List<MethodWrapper>();

			foreach (PropertyInfo prop in props)
                if (prop.GetIndexParameters().Length == 0)
					propertiesMap.Add(prop.Name, new PropertyWrapper(prop));

			foreach (FieldInfo field in fields)
			{
				bool setFiled = true;
				if (fieldsMap.TryGetValue(field.Name, out FieldWrapper existingFieldWrapper))
					setFiled = field.DeclaringType == type; //ponecham len ten field, ktory je deklarovany v current Type, t.j. prepisem ten, co je v rodicovi

				if (setFiled)
					fieldsMap[field.Name] = new FieldWrapper(field);
			}

			foreach (ConstructorInfo ctor in ctors)
				ctorsMap.Add(new MethodKey(ctor), new ConstructorWrapper(ctor));

			foreach (MethodInfo method in methods)
			{
				var key = new MethodKey(method);
				bool setMethod = true;
				if (methodsMap.TryGetValue(key, out MethodWrapper exisitngMethodWrapper))
					setMethod = method.DeclaringType == type; //ponecham len tu metodu, ktora je deklarovana v current Type, t.j. prepisem tu, co je v rodicovi
				
				if (setMethod)
					methodsMap[key] = new MethodWrapper(method);
			}

			foreach (MethodInfo genericMethod in genMethods)
			{
				genericMethods.Add(new MethodWrapper(genericMethod));
			}
			return new TypeWrapper(fieldsMap, propertiesMap, ctorsMap, methodsMap, genericMethods, type, suppressExceptions, allowMemberTypeCasting);
        }

        public object this[object target, string name]
        {
            get
            {
                return Getter(target, name);
            }
            set
            {
                Setter(target, name, value);
            }
		}

		private object SuppressOrThrowException(SuppressMessage suppressMessage, string memberName)
		{
			if (_suppressExceptions)
			{
				return null;
			}
			else
			{
				switch (suppressMessage)
				{
					case SuppressMessage.OutOfGange:
						throw new ArgumentOutOfRangeException(memberName);
					case SuppressMessage.InvalidRead:
						throw new InvalidOperationException($"Cannot read from member {memberName}");
					case SuppressMessage.InvalidWrite:
						throw new InvalidOperationException($"Cannot write to member {memberName}");
					default:
						throw new Exception($"Unknown exception occured for member {memberName}");
				}
			}
		}

		private object Getter(object target, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
				throw new ArgumentNullException(nameof(name));
            }

            string[] chunks = name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            object result = GetterInternal(target, chunks[0]);

            for (int i = 1; i < chunks.Length; i++)
			{
				if (result == null)
					return null;

				ObjectWrapper ow = ObjectWrapper.Create(result, true, null, _suppressExceptions, AllowMemberTypeCasting);
                result = ow[chunks[i]];
			}

            return result;
        }

        private object GetterInternal(object target, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
				throw new ArgumentNullException(nameof(name));
            }

			if (_propertiesMap.TryGetValue(name, out PropertyWrapper propertyWrapper))
				return propertyWrapper.PropertyInfo.CanRead
					? propertyWrapper.Get(target)
					: SuppressOrThrowException(SuppressMessage.InvalidRead, name);
			else
				return _fieldsMap.TryGetValue(name, out FieldWrapper fieldWrapper)
					? fieldWrapper.Get(target)
					: SuppressOrThrowException(SuppressMessage.OutOfGange, name);
		}

		private void Setter(object target, string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
				throw new ArgumentNullException(nameof(name));
            }

            string[] chunks = name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (chunks.Length == 1)
            {
                SetterInternal(target, chunks[0], value);
            }
            else
            {
				TypeWrapper currentTypeWrapper = this;
				object previousTarget = target;
                object currentTarget = null;

                for (int i = 0; i < chunks.Length - 1; i++)
                {
                    currentTarget = currentTypeWrapper.GetterInternal(previousTarget, chunks[i]);
                    if (currentTarget == null)
                    {
                        Type currentTargetType = currentTypeWrapper.GetMemberType(chunks[i]);
                        if (currentTargetType != null)
                        {
                            currentTarget = Activator.CreateInstance(currentTargetType);
                        }
                        else
                        {
							SuppressOrThrowException(SuppressMessage.OutOfGange, chunks[i]);
							return;
                        }
                    }

					currentTypeWrapper.SetterInternal(previousTarget, chunks[i], currentTarget);
					previousTarget = currentTarget;
					currentTypeWrapper = TypeWrapper.Create(currentTarget.GetType(), _suppressExceptions, AllowMemberTypeCasting);
				}

				currentTypeWrapper.SetterInternal(currentTarget, chunks[chunks.Length - 1], value);
			}
        }

        private void SetterInternal(object target, string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
				throw new ArgumentNullException(nameof(name));
            }
			if (_propertiesMap.TryGetValue(name, out PropertyWrapper propertyWrapper))
			{
				if (!propertyWrapper.PropertyInfo.CanWrite)
				{
					SuppressOrThrowException(SuppressMessage.InvalidWrite, name);
					return;
				}

				object convertedValue = value;
				if (AllowMemberTypeCasting)
				{
					Type valueType = value?.GetType();
					Type memType = propertyWrapper.PropertyInfo.GetReturnType();

					if (valueType != null && !memType.IsEquivalentNullableType(valueType))
						convertedValue = Convert.ChangeType(value, memType.GetUnderlyingNullableType());
				}

				propertyWrapper.Set(target, convertedValue);
			}
			else if (_fieldsMap.TryGetValue(name, out FieldWrapper fieldWrapper))
			{
				if (fieldWrapper.FieldInfo.IsLiteral) //read only field
				{
					SuppressOrThrowException(SuppressMessage.InvalidWrite, name);
					return;
				}

				object convertedValue = value;
				if (AllowMemberTypeCasting)
				{
					Type valueType = value?.GetType();
					Type memType = fieldWrapper.FieldInfo.GetReturnType();

					if (valueType != null && !memType.IsEquivalentNullableType(valueType))
						convertedValue = Convert.ChangeType(value, memType.GetUnderlyingNullableType());
				}

				fieldWrapper.Set(target, convertedValue);
			}
			else
				SuppressOrThrowException(SuppressMessage.OutOfGange, name);
		}
		
		public object CreateInstance()
			=> Instantiator(null, null);

		public object CreateInstance(Type methodParameterType, object methodParameter)
			=> Instantiator(new Type[] { methodParameterType }, new object[] { methodParameter });

		public object CreateInstance(Type[] methodParameterTypes, object[] methodParameters)
			=> Instantiator(methodParameterTypes, methodParameters);

		private object Instantiator(Type[] methodParameterTypes, object[] methodParameters)
		{
			if (methodParameterTypes?.Length != methodParameters?.Length)
				throw new ArgumentException($"LENGTH of {nameof(methodParameterTypes)} && {nameof(methodParameters)}");

			if (_ctorsMap.TryGetValue(new MethodKey(CTOR_NAME, methodParameterTypes), out ConstructorWrapper constructorWrapper))
				return constructorWrapper.Invoke(methodParameters);
			else
				return SuppressOrThrowException(SuppressMessage.OutOfGange, CTOR_NAME);
		}

		public object InvokeMethod(object target, string methodName)
			=> Invoker(target, methodName, null, null, null);

		public object InvokeMethod(object target, string methodName, Type methodParameterType, object methodParameter)
			=> Invoker(target, methodName, null, new Type[] { methodParameterType }, new object[] { methodParameter });

		public object InvokeMethod(object target, string methodName, Type[] methodParameterTypes, object[] methodParameters)
			=> Invoker(target, methodName, null, methodParameterTypes, methodParameters);

		public object InvokeGenericMethod(object target, string methodName, Type[] methodGenericTypes)
			=> Invoker(target, methodName, methodGenericTypes, null, null);

		public object InvokeGenericMethod(object target, string methodName, Type[] methodGenericTypes, Type methodParameterType, object methodParameter)
			=> Invoker(target, methodName, methodGenericTypes, new Type[] { methodParameterType }, new object[] { methodParameter });

		public object InvokeGenericMethod(object target, string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, object[] methodParameters)
			=> Invoker(target, methodName, methodGenericTypes, methodParameterTypes, methodParameters);

		private object Invoker(object target, string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, object[] methodParameters)
		{
			if (string.IsNullOrWhiteSpace(methodName))
			{
				throw new ArgumentNullException(nameof(methodName));
			}

			string[] chunks = methodName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length == 1)
			{
				return InvokeMethodInternal(target, methodName, methodGenericTypes, methodParameterTypes, methodParameters);
			}
			else
			{
				object result = GetterInternal(target, chunks[0]);
				if (result == null)
					return null;

				ObjectWrapper ow = null;
				for (int i = 1; i < chunks.Length - 1; i++)
				{
					ow = ObjectWrapper.Create(result, true, null, _suppressExceptions, AllowMemberTypeCasting);
					result = ow[chunks[i]];

					if (result == null)
						return null;
				}

				ow = ObjectWrapper.Create(result, true, null, _suppressExceptions, AllowMemberTypeCasting);
				return ow.InvokeMethod(methodName, methodParameterTypes, methodParameters);
			}
		}

		private object InvokeMethodInternal(object target, string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, object[] methodParameters)
		{
			if (string.IsNullOrWhiteSpace(methodName))
				throw new ArgumentNullException(nameof(methodName));

			if (methodParameterTypes?.Length != methodParameters?.Length)
				throw new ArgumentException($"LENGTH of {nameof(methodParameterTypes)} && {nameof(methodParameters)}");

			MethodWrapper methodWrapper = null;

			if (methodGenericTypes == null || methodGenericTypes.Length == 0)
			{
				if (_methodsMap.TryGetValue(new MethodKey(methodName, methodParameterTypes), out methodWrapper))
					return methodWrapper.Invoke(target, methodParameters);
			}
			else
			{
				methodWrapper = FindGenericMethodWrapper(methodName, methodGenericTypes, methodParameterTypes, out MethodInfo genericMethodInfo);
				if (methodWrapper != null)
					return methodWrapper.Invoke(genericMethodInfo, target, methodParameters);
			}

			return SuppressOrThrowException(SuppressMessage.OutOfGange, methodName);
		}

		public async Task<object> InvokeMethodAsync(object target, string methodName)
			=> await InvokerAsync(target, methodName, null, null, null);

		public async Task<object> InvokeMethodAsync(object target, string methodName, Type methodParameterType, object methodParameter)
			=> await InvokerAsync(target, methodName, null, new Type[] { methodParameterType }, new object[] { methodParameter });

		public async Task<object> InvokeMethodAsync(object target, string methodName, Type[] methodParameterTypes, object[] methodParameters)
			=> await InvokerAsync(target, methodName, null, methodParameterTypes, methodParameters);

		public async Task<object> InvokeGenericMethodAsync(object target, string methodName, Type[] methodGenericTypes)
			=> await InvokerAsync(target, methodName, methodGenericTypes, null, null);

		public async Task<object> InvokeGenericMethodAsync(object target, string methodName, Type[] methodGenericTypes, Type methodParameterType, object methodParameter)
			=> await InvokerAsync(target, methodName, methodGenericTypes, new Type[] { methodParameterType }, new object[] { methodParameter });

		public async Task<object> InvokeGenericMethodAsync(object target, string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, object[] methodParameters)
			=> await InvokerAsync(target, methodName, methodGenericTypes, methodParameterTypes, methodParameters);

		private async Task<object> InvokerAsync(object target, string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, object[] methodParameters)
		{
			if (string.IsNullOrWhiteSpace(methodName))
			{
				throw new ArgumentNullException(nameof(methodName));
			}

			string[] chunks = methodName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			if (chunks.Length == 1)
			{
				return await InvokeMethodInternalAsync(target, methodName, methodGenericTypes, methodParameterTypes, methodParameters);
			}
			else
			{
				object result = GetterInternal(target, chunks[0]);
				if (result == null)
					return null;

				ObjectWrapper ow = null;
				for (int i = 1; i < chunks.Length - 1; i++)
				{
					ow = ObjectWrapper.Create(result, true, null, _suppressExceptions, AllowMemberTypeCasting);
					result = ow[chunks[i]];

					if (result == null)
						return null;
				}

				ow = ObjectWrapper.Create(result, true, null, _suppressExceptions, AllowMemberTypeCasting);
				return await ow.InvokeMethodAsync(methodName, methodParameterTypes, methodParameters);
			}
		}

		private async Task<object> InvokeMethodInternalAsync(object target, string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, object[] methodParameters)
		{
			if (string.IsNullOrWhiteSpace(methodName))
				throw new ArgumentNullException(nameof(methodName));

			if (methodParameterTypes?.Length != methodParameters?.Length)
				throw new ArgumentException($"LENGTH of {nameof(methodParameterTypes)} && {nameof(methodParameters)}");

			MethodWrapper methodWrapper = null;

			if (methodGenericTypes == null || methodGenericTypes.Length == 0)
			{
				if (_methodsMap.TryGetValue(new MethodKey(methodName, methodParameterTypes), out methodWrapper))
					return await methodWrapper.InvokeAsync(target, methodParameters);
			}
			else
			{
				methodWrapper = FindGenericMethodWrapper(methodName, methodGenericTypes, methodParameterTypes, out MethodInfo genericMethodInfo);
				if (methodWrapper != null)
					return await methodWrapper.InvokeAsync(genericMethodInfo, target, methodParameters);
			}

			return SuppressOrThrowException(SuppressMessage.OutOfGange, methodName);
		}

		private MethodWrapper FindGenericMethodWrapper(string methodName, Type[] methodGenericTypes, Type[] methodParameterTypes, out MethodInfo genericMethodInfo)
		{
			genericMethodInfo = null;

			if (methodGenericTypes == null || methodGenericTypes.Length == 0)
				return null;

			foreach (var genericMethodWrapper in _genericMethods.Where(mw =>
														mw.MethodName == methodName
														&& mw.GenericParametersCount == methodGenericTypes.Length
														&& mw.Parameters?.Length == methodParameterTypes?.Length))
			{
				var genMethodInfo = genericMethodWrapper.MethodInfo.MakeGenericMethod(methodGenericTypes);
				bool parametersMatch = true;
				if (0 < genericMethodWrapper.Parameters?.Length)
				{
					var parameters = genMethodInfo.GetParameters();
					for (int i = 0; i < parameters.Length; i++)
						if (parameters[i].ParameterType != methodParameterTypes[i])
						{
							parametersMatch = false;
							break;
						}
				}

				if (parametersMatch)
				{
					genericMethodInfo = genMethodInfo;
					return genericMethodWrapper;
				}
			}

			return null;
		}

		public Dictionary<string, Type> GetAllMemberTypes()
		{
			var result = _propertiesMap.ToDictionary(x => x.Key, y => y.Value.PropertyInfo.Type());
			foreach (var kvp in _fieldsMap)
			{
				result[kvp.Key] = kvp.Value.FieldInfo.Type();
			}
			return result;
		}

		public List<string> GetAllMemberNames()
		{
			var result = _propertiesMap.Keys.ToList();
			result.AddRange(_fieldsMap.Keys);
			return result;
		}

		public MemberInfo GetMember(string memberName)
		{
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (_propertiesMap.TryGetValue(memberName, out PropertyWrapper propertyWrapper))
			{
				return propertyWrapper.PropertyInfo;
			}
			else
			if (_fieldsMap.TryGetValue(memberName, out FieldWrapper fieldWrapper))
			{
				return fieldWrapper.FieldInfo;
			}
			else
			{
				return null;
			}
		}

		public Type GetMemberType(string memberName)
		{
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (_propertiesMap.TryGetValue(memberName, out PropertyWrapper propertyWrapper))
			{
				return propertyWrapper.PropertyInfo.PropertyType;
			}
			else
			if (_fieldsMap.TryGetValue(memberName, out FieldWrapper fieldWrapper))
			{
				return fieldWrapper.FieldInfo.FieldType;
			}
			else
			{
				return null;
			}
		}

		public Dictionary<string, Type> GetAllPropertyTypes()
        {
            return _propertiesMap.ToDictionary(x => x.Key, y => y.Value.PropertyInfo.Type());
		}

		public List<string> GetAllPropertyNames()
		{
			return _propertiesMap.Keys.ToList();
		}

		public List<PropertyInfo> GetAllPropertyInfos()
		{
			return _propertiesMap.Values.Select(x => x.PropertyInfo).ToList();
		}

		public PropertyInfo GetPropertyInfo(string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException(nameof(propertyName));

			_propertiesMap.TryGetValue(propertyName, out PropertyWrapper propertyWrapper);
			return propertyWrapper.PropertyInfo;
		}

        public Type GetPropertyType(string propertyName)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(propertyName);
            if (propertyInfo == null)
			{
				throw new Exception($"{nameof(propertyInfo)} == null");
			}
            return propertyInfo.PropertyType;
        }

        public Dictionary<string, Type> GetPublicPropertyTypes()
        {
            PropertyInfo[] props = Type.GetProperties(Flags.InstancePublic);
            if (props == null)
            {
                return new Dictionary<string, Type>();
            }
            return props.ToDictionary(x => x.Name, y => y.Type());
        }

        public Dictionary<string, Type> GetPublicPropertyTypesWithSetters()
        {
            PropertyInfo[] props = Type.GetProperties(Flags.InstancePublic);
            if (props == null)
            {
                return new Dictionary<string, Type>();
            }
            return props.Where(x => x.CanWrite).ToDictionary(x => x.Name, y => y.Type());
        }

        public Dictionary<string, Type> GetAllFieldTypes()
        {
            return _fieldsMap.ToDictionary(x => x.Key, y => y.Value.FieldInfo.Type());
		}

		public List<string> GetAllFieldNames()
		{
			return _fieldsMap.Keys.ToList();
		}

		public List<FieldInfo> GetAllFieldInfos()
		{
			return _fieldsMap.Values.Select(x => x.FieldInfo).ToList();
		}

		public FieldInfo GetFieldInfo(string fieldName)
		{
			if (string.IsNullOrWhiteSpace(fieldName))
				throw new ArgumentNullException(nameof(fieldName));

			_fieldsMap.TryGetValue(fieldName, out FieldWrapper fieldWrapper);
			return fieldWrapper.FieldInfo;
        }

        public Type GetFieldType(string fieldName)
        {
            FieldInfo fieldInfo = GetFieldInfo(fieldName);
            if (fieldInfo == null)
            {
				throw new Exception($"{nameof(fieldInfo)} == null");
            }
            return fieldInfo.FieldType;
        }

        public Dictionary<string, Type> GetPublicFieldTypes()
        {
            FieldInfo[] fields = Type.GetFields(Flags.InstancePublic);
            if (fields == null)
            {
                return new Dictionary<string, Type>();
            }
            return fields.ToDictionary(x => x.Name, y => y.Type());
		}

		public List<ConstructorInfo> GetAllConstructorInfos()
		{
			return _ctorsMap.Values.Select(x => x.ConstructorInfo).ToList();
		}

		public ConstructorInfo GetConstructorInfo(params Type[] paramTypes)
		{
			_ctorsMap.TryGetValue(new MethodKey(CTOR_NAME, paramTypes), out ConstructorWrapper constructorWrapper);
			return constructorWrapper.ConstructorInfo;
		}

		public List<MethodInfo> GetAllMethodInfos()
		{
			var result = _methodsMap.Values.Select(x => x.MethodInfo).ToList();
			result.AddRange(_genericMethods.Select(gm => gm.MethodInfo));
			return result;
		}

		public MethodInfo GetMethodInfo(string methodName, params Type[] paramTypes)
		{
			if (string.IsNullOrWhiteSpace(methodName))
				throw new ArgumentNullException(nameof(methodName));

			_methodsMap.TryGetValue(new MethodKey(methodName, paramTypes), out MethodWrapper methodWrapper);
			return methodWrapper.MethodInfo;
		}

		public MethodInfo GetGenericMethodInfo(string methodName, Type[] genericTypes, params Type[] paramTypes)
		{
			if (string.IsNullOrWhiteSpace(methodName))
				throw new ArgumentNullException(nameof(methodName));

			FindGenericMethodWrapper(methodName, genericTypes, paramTypes, out MethodInfo genericMethodInfo);
			return genericMethodInfo;
		}

		public PropertyWrapper GetPropertyWrapper(string propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException(nameof(propertyName));

			_propertiesMap.TryGetValue(propertyName, out PropertyWrapper propertyWrapper);
			return propertyWrapper;
		}

		public FieldWrapper GetFieldWrapper(string fieldName)
		{
			if (string.IsNullOrWhiteSpace(fieldName))
				throw new ArgumentNullException(nameof(fieldName));

			_fieldsMap.TryGetValue(fieldName, out FieldWrapper fieldWrapper);
			return fieldWrapper;
		}

		public ConstructorWrapper GetConstructorWrapper(params Type[] paramTypes)
		{
			_ctorsMap.TryGetValue(new MethodKey(CTOR_NAME, paramTypes), out ConstructorWrapper constructorWrapper);
			return constructorWrapper;
		}

		public MethodWrapper GetMethodWrapper(string methodName, params Type[] paramTypes)
		{
			if (string.IsNullOrWhiteSpace(methodName))
				throw new ArgumentNullException(nameof(methodName));

			_methodsMap.TryGetValue(new MethodKey(methodName, paramTypes), out MethodWrapper methodWrapper);
			return methodWrapper;
		}

		public MethodWrapper GetGenericMethodWrapper(string methodName, Type[] genericTypes, Type[] paramTypes, out MethodInfo genericMethodInfo)
		{
			if (string.IsNullOrWhiteSpace(methodName))
				throw new ArgumentNullException(nameof(methodName));

			return FindGenericMethodWrapper(methodName, genericTypes, paramTypes, out genericMethodInfo);
		}
		
		public ObjectWrapper CreateObjectWrapper(
			bool includeAllBaseTypes = false,
			List<System.Type> allowedBaseTypes = null,
			bool suppressExceptions = false,
			bool allowMemberTypeCasting = false)
		{
			return ObjectWrapper.Create(
				this.CreateInstance(),
				this,
				includeAllBaseTypes,
				allowedBaseTypes,
				suppressExceptions,
				allowMemberTypeCasting);
		}
		
		public ObjectWrapper CreateObjectWrapper(
			Type methodParameterType,
			object methodParameter,
			bool includeAllBaseTypes = false,
			List<System.Type> allowedBaseTypes = null,
			bool suppressExceptions = false,
			bool allowMemberTypeCasting = false)
		{
			return ObjectWrapper.Create(
				this.CreateInstance(methodParameterType, methodParameter),
				this,
				includeAllBaseTypes,
				allowedBaseTypes,
				suppressExceptions,
				allowMemberTypeCasting);
		}

		public ObjectWrapper CreateObjectWrapper(
			Type[] methodParameterTypes,
			object[] methodParameters,
			bool includeAllBaseTypes = false,
			List<System.Type> allowedBaseTypes = null,
			bool suppressExceptions = false,
			bool allowMemberTypeCasting = false)
		{
			return ObjectWrapper.Create(
				this.CreateInstance(methodParameterTypes, methodParameters),
				this,
				includeAllBaseTypes,
				allowedBaseTypes,
				suppressExceptions,
				allowMemberTypeCasting);
		}
	}
}
