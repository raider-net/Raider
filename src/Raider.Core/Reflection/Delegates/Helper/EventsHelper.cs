using System.Reflection;

namespace Raider.Reflection.Delegates.Helper
{
	internal static partial class EventsHelper
	{
		public static readonly MethodInfo EventHandlerFactoryMethodInfo =
			typeof(EventsHelper).GetMethod("EventHandlerFactory")!;
	}
}
