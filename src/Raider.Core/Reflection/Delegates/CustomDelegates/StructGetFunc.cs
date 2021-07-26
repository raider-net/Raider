namespace Raider.Reflection.Delegates.CustomDelegates
{
	/// <summary>
	///     Raider.Reflection.Delegates for returning value of property from structure type by reference.
	/// </summary>
	/// <typeparam name="T">Type of structure</typeparam>
	/// <typeparam name="TProp">Property type</typeparam>
	/// <param name="i">Structure type instance</param>
	/// <returns>Value of a property</returns>
	public delegate TProp StructGetFunc<T, out TProp>(ref T i) where T : struct;
}
