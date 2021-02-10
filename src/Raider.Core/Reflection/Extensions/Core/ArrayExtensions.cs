using System;
using Raider.Reflection.Emitter;

namespace Raider.Reflection
{
    /// <summary>
    /// Extension methods for working with arrays.
    /// </summary>
    public static class ArrayExtensions
    {
        #region Array Access
        /// <summary>
        /// Sets <paramref name="value"/> to the element at position <paramref name="index"/> of <paramref name="array"/>.
        /// </summary>
        /// <returns><paramref name="array"/>.</returns>
        internal static object SetElement( this object array, long index, object value )
        {
            ((Array) array).SetValue( value, index );
            return array;
        }

        /// <summary>
        /// Gets the element at position <paramref name="index"/> of <paramref name="array"/>.
        /// </summary>
        internal static object GetElement( this object array, long index )
        {
            return ((Array) array).GetValue( index );
        }

        /// <summary>
        /// Creates a delegate which can set element of <paramref name="arrayType"/>.
        /// </summary>
        public static ArrayElementSetter DelegateForSetElement( this Type arrayType )
        {
            return (ArrayElementSetter) new ArraySetEmitter( arrayType ).GetDelegate();
        }

        /// <summary>
        /// Creates a delegate which can retrieve element of <paramref name="arrayType"/>.
        /// </summary>
        public static ArrayElementGetter DelegateForGetElement( this Type arrayType )
        {
            return (ArrayElementGetter) new ArrayGetEmitter( arrayType ).GetDelegate();
        }
        #endregion
    }
}