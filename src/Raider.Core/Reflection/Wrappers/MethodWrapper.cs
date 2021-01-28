using Raider.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Raider.Reflection
{
	/* USAGE:
	 
		public string Metoda(ref int ii, TestEnum? en, out double dd, DateTime? dt = null)
		{
			dd = 666;
			Console.WriteLine($"{nameof(NewClass<TData>)}.{nameof(Metoda)}({ii}, {en}, {dt}) CALL");
			ii = 6;
			return "Final result.";
		}

		var ow = ObjectWrapper.Create(new MyClass());
		var mw = ow.GetMethodWrapper(
						nameof(NewClass<string>.Metoda),
						typeof(int).MakeByRefType(),      //ref
						typeof(TestEnum?),
						typeof(double).MakeByRefType(),   //out
						typeof(DateTime?));

		object[] arguments = new object[] { 2, TestEnum.hodnota3, 5, DateTime.Now };
		var result = mw.Invoke(newClass, arguments);
	 */

	public sealed class MethodWrapper
	{
		public static readonly Type VoidType = typeof(void);
		public static readonly Type TaskType = typeof(Task);

		private MethodInvoker _invoker;

		public MethodInfo MethodInfo { get; }
		public bool IsAsync { get; }
		public Type ReturnType => MethodInfo.ReturnType;
		public bool IsVoid => ReturnType == VoidType;
		public bool IsVoidAsync => ReturnType == TaskType;
		public string MethodName => MethodInfo.Name;
		public MethodInvoker Invoker => GetInvoker();
		public ParameterInfo[] Parameters { get; }
		public int GenericParametersCount { get; }

		public MethodWrapper(MethodInfo methodInfo)
		{
			MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			IsAsync = MethodInfo.IsAwaitable();
			Parameters = MethodInfo.GetParameters();
			if (MethodInfo.IsGenericMethodDefinition)
				GenericParametersCount = MethodInfo.GetGenericArguments().Length;
		}

		private MethodInvoker GetInvoker()
		{
			if (_invoker != null)
				return _invoker;

			_invoker = MethodInfo.DelegateForCallMethod();
			return _invoker;
		}

		public MethodWrapper Initialize()
		{
			GetInvoker();
			return this;
		}

		public object Invoke(object target, params object[] methodParameters)
			=> Invoker(
					MethodInfo.IsStatic ? null : target,
					(methodParameters == null || methodParameters.Length == 0)
						? new object[MethodInfo.Parameters().Count]
						: methodParameters);

		public object Invoke(object target, Type[] methodGenericTypes, params object[] methodParameters)
			=> Invoke(MethodInfo.MakeGenericMethod(methodGenericTypes), target, methodParameters);

		internal object Invoke(MethodInfo genericMethodInfo, object target, params object[] methodParameters)
			=> genericMethodInfo?.DelegateForCallMethod()(
					genericMethodInfo.IsStatic ? null : target,
					(methodParameters == null || methodParameters.Length == 0)
						? new object[genericMethodInfo.Parameters().Count]
						: methodParameters);

		public async Task<object> InvokeAsync(object target, params object[] methodParameters)
		{
			if (IsVoidAsync)
			{
				Invoker(
					   MethodInfo.IsStatic ? null : target,
					   (methodParameters == null || methodParameters.Length == 0)
						   ? new object[MethodInfo.Parameters().Count]
						   : methodParameters);
				return null;
			}
			else
			{
				return await (dynamic)Invoker(
						MethodInfo.IsStatic ? null : target,
						(methodParameters == null || methodParameters.Length == 0)
							? new object[MethodInfo.Parameters().Count]
							: methodParameters);
			}
		}

		public async Task<object> InvokeAsync(object target, Type[] methodGenericTypes, params object[] methodParameters)
			=> await InvokeAsync(MethodInfo.MakeGenericMethod(methodGenericTypes), target, methodParameters);

		internal async Task<object> InvokeAsync(MethodInfo genericMethodInfo, object target, params object[] methodParameters)
		{
			if (IsVoidAsync)
			{
				genericMethodInfo?.DelegateForCallMethod()(
						genericMethodInfo.IsStatic ? null : target,
						(methodParameters == null || methodParameters.Length == 0)
							? new object[genericMethodInfo.Parameters().Count]
							: methodParameters);
				return null;
			}
			else
			{
				return await (dynamic)genericMethodInfo?.DelegateForCallMethod()(
						genericMethodInfo.IsStatic ? null : target,
						(methodParameters == null || methodParameters.Length == 0)
							? new object[genericMethodInfo.Parameters().Count]
							: methodParameters);
			}
		}

		public object InvokeSyncOverAsync(object target, params object[] methodParameters)
		{
			return this.IsAsync
				? Task
					.Run<object>(async () => await InvokeAsync(target, (object[])methodParameters))
					.GetAwaiter()
					.GetResult()
				: Invoke(target, (object[])methodParameters);
		}

		public object InvokeSyncOverAsync(object target, Type[] methodGenericTypes, params object[] methodParameter)
		{
			return this.IsAsync
				? Task
					.Run<object>(async () => await InvokeAsync(target, (Type[])methodGenericTypes, (object[])methodParameter))
					.GetAwaiter()
					.GetResult()
				: Invoke(target, (Type[])methodGenericTypes, (object[])methodParameter);
		}

		internal object InvokeSyncOverAsync(MethodInfo genericMethodInfo, object target, params object[] methodParameters)
		{
			return this.IsAsync
				? Task
					.Run<object>(async () => await InvokeAsync(genericMethodInfo, target, (object[])methodParameters))
					.GetAwaiter()
					.GetResult()
				: Invoke(genericMethodInfo, target, (object[])methodParameters);
		}

		public async Task<object> InvokeAsyncOverAsync(object target, params object[] methodParameters)
		{
			return this.IsAsync
				? await InvokeAsync(target, (object[])methodParameters)
				//: await Task.Run<object>(() => Invoke(target, (object[])methodParameters));
				: Invoke(target, (object[])methodParameters);
		}

		public async Task<object> InvokeAsyncOverAsync(object target, Type[] methodGenericTypes, params object[] methodParameter)
		{
			return this.IsAsync
				? await InvokeAsync(target, (Type[])methodGenericTypes, (object[])methodParameter)
				//: await Task.Run<object>(() => Invoke(target, (Type[])methodGenericTypes, (object[])methodParameter));
				: Invoke(target, (Type[])methodGenericTypes, (object[])methodParameter);
		}

		internal async Task<object> InvokeAsyncOverAsync(MethodInfo genericMethodInfo, object target, params object[] methodParameters)
		{
			return this.IsAsync
				? await InvokeAsync(genericMethodInfo, target, (object[])methodParameters)
				//: await Task.Run<object>(() => Invoke(genericMethodInfo, target, (object[])methodParameters));
				: Invoke(genericMethodInfo, target, (object[])methodParameters);
		}
	}
	
	public sealed class MethodKey : IEquatable<MethodKey>
	{
		public string Name { get; }
		public Type[] ParamTypes { get; }

		public MethodKey(string name, Type[] paramTypes)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			ParamTypes = paramTypes ?? new Type[] { };
		}

		public MethodKey(MethodBase methodBase)
			: this(methodBase?.Name, methodBase?.GetParameters().Select(p => p.ParameterType).ToArray())
		{
		}

		public bool Equals(MethodKey other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(other, this))
				return true;

			if (other.Name != Name
				|| other.ParamTypes.Length != ParamTypes.Length)
				return false;

			for (int i = 0; i < ParamTypes.Length; i++)
				if (ParamTypes[i] != other.ParamTypes[i])
					return false;

			return true;
		}

		public override bool Equals(object obj)
			=> Equals(obj as MethodKey);

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (int)2166136261;
				hash = (hash * 16777619) ^ Name.GetHashCode();
				for (int i = 0; i < ParamTypes.Length; i++)
					hash = (hash * 16777619) ^ ParamTypes[i].GetHashCode();
				return hash;
			}
		}

		public static bool operator ==(MethodKey left, MethodKey right)
		{
			if (left is null)
				return right is null;

			return left.Equals(right);
		}

		public static bool operator !=(MethodKey left, MethodKey right)
		{
			if (left is null)
				return !(right is null);

			return !left.Equals(right);
		}
	}
}
