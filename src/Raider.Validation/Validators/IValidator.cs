namespace Raider.Validation
{
	public interface IValidator<T>
	{
		IValidationDescriptor ToDescriptor();

		ValidationResult Validate(T? obj);

		//TODO
		//Task<ValidationResult> ValidateAsync(object? obj, CancellationToken cancellation);
	}
}
