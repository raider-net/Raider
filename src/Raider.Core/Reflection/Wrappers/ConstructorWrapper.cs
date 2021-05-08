//using System;
//using System.Reflection;

//namespace Raider.Reflection
//{
//	public sealed class ConstructorWrapper
//	{
//		private ConstructorInvoker _invoker;

//		public ConstructorInfo ConstructorInfo { get; }
//		public ConstructorInvoker Invoker => GetInvoker();
//		public ParameterInfo[] Parameters { get; }

//		public ConstructorWrapper(ConstructorInfo constructorInfo)
//		{
//			ConstructorInfo = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
//			Parameters = ConstructorInfo.GetParameters();
//		}

//		private ConstructorInvoker GetInvoker()
//		{
//			if (_invoker != null)
//				return _invoker;

//			_invoker = ConstructorInfo.DelegateForCreateInstance();
//			return _invoker;
//		}

//		public ConstructorWrapper Initialize()
//		{
//			GetInvoker();
//			return this;
//		}

//		public object Invoke(params object[] constructorParameters)
//			=> Invoker((constructorParameters == null || constructorParameters.Length == 0)
//						? new object[ConstructorInfo.Parameters().Count]
//						: constructorParameters);
//	}
//}
