namespace Raider.Reflection.Delegates.CustomDelegates
{
	/// <summary>
	///     Raider.Reflection.Delegates for setting value of indexer with unspecified index parameters in structure type.
	/// </summary>
	/// <typeparam name="T">Structure type</typeparam>
	/// <typeparam name="TProp">Property type</typeparam>
	/// <param name="i">Structure type instance</param>
	/// <param name="value">Value of indexer to set at given index parameters</param>
	/// <returns>Changed structure value</returns>
	public delegate T StructSetAction<T, in TProp>(T i, TProp value);
}
