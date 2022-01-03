namespace Raider.Validation
{
	public interface IValidationFrame
	{
		string? ObjectType { get; }
		string? PropertyName { get; }
		string? PropertyNameWithIndex { get; }
		IValidationFrame? Parent { get; }
		int? Index { get; }
		int Depth { get; }
	}
}
