namespace Raider.Validation
{
	public interface IValidator
	{
		ValidationFrame ValidationFrame { get; }
		ValidatorType ValidatorType { get; }
		bool Conditional { get; }

		IValidationDescriptor ToDescriptor();
		ValidationResult Validate(object? obj);
	}
}
