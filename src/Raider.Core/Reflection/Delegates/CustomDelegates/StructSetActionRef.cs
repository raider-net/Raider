﻿namespace Raider.Reflection.Delegates
{
	/// <summary>
	///     Raider.Reflection.Delegates for setting value of indexer with single index parameter in structure type by reference.
	/// </summary>
	/// <typeparam name="T">Structure type</typeparam>
	/// <typeparam name="TProp">Property type</typeparam>
	/// <param name="i">Structure type instance</param>
	/// <param name="value">Value of indexer to set at given index parameters</param>
	public delegate void StructSetActionRef<T, in TProp>(ref T i, TProp value);
}
