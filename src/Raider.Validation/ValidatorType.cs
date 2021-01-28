namespace Raider.Validation
{
	public enum ValidatorType
	{
		NONE = 0,
		Email,
		DefaultOrEmptyClass,
		DefaultOrEmptyStruct,
		NotDefaultOrEmptyClass,
		NotDefaultOrEmptyStruct,
		Equal,
		NotEqual,
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
