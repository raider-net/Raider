using Raider.Validation.Client;

namespace Raider.Validation
{
	public interface IValidationFailure : IBaseValidationFailure
	{
		ValidatorType Type { get; }
		IClientConditionDefinition? ClientConditionDefinition { get; }
	}
}
