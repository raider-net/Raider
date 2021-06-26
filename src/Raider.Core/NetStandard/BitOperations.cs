#define SYSTEM_PRIVATE_CORELIB

#if NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

using System.Runtime.CompilerServices;

namespace System.Numerics
{
	/// <summary>
	/// Utility methods for intrinsic bit-twiddling operations.
	/// The methods use hardware intrinsics when available on the underlying platform,
	/// otherwise they use optimized software fallbacks.
	/// </summary>
#if SYSTEM_PRIVATE_CORELIB
    public
#else
	internal
#endif
		static class BitOperations
	{
		/// <summary>
		/// Rotates the specified value left by the specified number of bits.
		/// Similar in behavior to the x86 instruction ROL.
		/// </summary>
		/// <param name="value">The value to rotate.</param>
		/// <param name="offset">The number of bits to rotate by.
		/// Any value outside the range [0..31] is treated as congruent mod 32.</param>
		/// <returns>The rotated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static uint RotateLeft(uint value, int offset)
			=> (value << offset) | (value >> (32 - offset));

		/// <summary>
		/// Rotates the specified value left by the specified number of bits.
		/// Similar in behavior to the x86 instruction ROL.
		/// </summary>
		/// <param name="value">The value to rotate.</param>
		/// <param name="offset">The number of bits to rotate by.
		/// Any value outside the range [0..63] is treated as congruent mod 64.</param>
		/// <returns>The rotated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static ulong RotateLeft(ulong value, int offset)
			=> (value << offset) | (value >> (64 - offset));

		/// <summary>
		/// Rotates the specified value right by the specified number of bits.
		/// Similar in behavior to the x86 instruction ROR.
		/// </summary>
		/// <param name="value">The value to rotate.</param>
		/// <param name="offset">The number of bits to rotate by.
		/// Any value outside the range [0..31] is treated as congruent mod 32.</param>
		/// <returns>The rotated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static uint RotateRight(uint value, int offset)
			=> (value >> offset) | (value << (32 - offset));

		/// <summary>
		/// Rotates the specified value right by the specified number of bits.
		/// Similar in behavior to the x86 instruction ROR.
		/// </summary>
		/// <param name="value">The value to rotate.</param>
		/// <param name="offset">The number of bits to rotate by.
		/// Any value outside the range [0..63] is treated as congruent mod 64.</param>
		/// <returns>The rotated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[CLSCompliant(false)]
		public static ulong RotateRight(ulong value, int offset)
			=> (value >> offset) | (value << (64 - offset));
	}
}
#endif
