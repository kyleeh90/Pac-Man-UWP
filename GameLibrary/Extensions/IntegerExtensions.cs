using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    /// <summary>
    /// Extension methods for integer types.
    /// </summary>
    public static class IntegerExtensions
    {
        /// <summary>
        /// Rotates the bits of an unsigned integer to the right.
        /// </summary>
        /// <param name="shiftAmount"></param>
        /// <returns>The uint rotated by the shift amount to the right</returns>
        public static uint BitRotateRight(this uint value, int shiftAmount)
        {
            return (value >> shiftAmount) | (value << (32 - shiftAmount));
        }

        /// <summary>
        /// Gets the bit at the specified index.
        /// </summary>
        /// <remarks>Uses the least significant bit as the first index.</remarks>
        /// <param name="bitIndex">The bit index to get.</param>
        /// <returns>1 if the bit is set, otherwise 0.</returns>
        public static uint GetBit(this uint value, int bitIndex)
        {
            return (value >> bitIndex - 1) & 1;
        }
    }
}
