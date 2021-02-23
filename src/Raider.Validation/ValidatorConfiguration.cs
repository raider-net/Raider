using Microsoft.Extensions.Localization;

namespace Raider.Validation
{
	public static class ValidatorConfiguration
	{
		//public static Func<Type, MemberInfo, LambdaExpression, string>? DisplayNameResolver { get; }
		public static IStringLocalizer? Localizer { get; set; }

		//public ValidatorConfiguration(IStringLocalizerFactory? factory, Func<Type, MemberInfo, LambdaExpression, string>? displayNameResolver)
		//{
		//	if (factory != null)
		//		Localizer = factory.Create(Resources.ValidationKeys.__BaseName, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

		//	DisplayNameResolver = displayNameResolver;
		//}
	}
}
