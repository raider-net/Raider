namespace Raider.Reflection
{
	/// <summary>
	/// A null struct for actions which do not return a TResult.
	/// </summary>
	internal struct EmptyStruct
	{
		internal static readonly EmptyStruct Instance = new EmptyStruct();
	}
}
