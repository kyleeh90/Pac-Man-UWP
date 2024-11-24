using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    /// <summary>
    /// Extension methods for the Double class.
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Squares this double.
        /// </summary>
        /// <returns>This double squared</returns>
        public static double Squared(this double value) => value * value;
    }
}
