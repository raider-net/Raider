namespace Raider.Validation
{
	public enum ValidatorType
	{
		Validator = 0,
		ConditionalValidator,
		NavigationValidator,
		EnumerableValidator,
		SwitchValidator,
		PropertyValidator,
		Email,
		DefaultOrEmpty,
		NotDefaultOrEmpty,
		Equal,
		NotEqual,
		MultiEqual,
		MultiNotEqual,
		ExclusiveBetween,
		GreaterThanOrEqual,
		GreaterThan,
		InclusiveBetween,
		Length,
		LessThanOrEqual,
		LessThan,
		Null,
		NotNull,
		PrecisionScale,
		RegEx,
		ErrorObject,
		ErrorProperty
	}
}
