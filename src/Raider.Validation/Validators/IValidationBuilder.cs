namespace Raider.Validation
{
	public interface IValidationBuilder<T> : IValidationDescriptorBuilder
	{
		Validator<T> BuildRules();

		Validator<T> BuildRules(Validator<T> parent);
	}
}
