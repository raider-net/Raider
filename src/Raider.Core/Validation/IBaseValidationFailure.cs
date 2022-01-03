namespace Raider.Validation
{
	public interface IBaseValidationFailure
	{
		IValidationFrame ValidationFrame { get; }
		ValidationSeverity Severity { get; }
		string Message { get; }
		string MessageWithPropertyName { get; }
		bool Conditional { get; }
	}
}
