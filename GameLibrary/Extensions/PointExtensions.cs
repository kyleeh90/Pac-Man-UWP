using System;
using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// Extension methods for the Point struct.
    /// </summary>
    public static class PointExtensions
    {
        /// <summary>
        /// A Point with both values at zero.
        /// </summary>
        public static Point ZERO = new Point(0, 0);

        /// <summary>
        /// Add a Point to this Point.
        /// </summary>
        /// <param name="other">The Point to add to this Point.</param>
        /// <returns>A new Point which is the product of adding the other Point to this Point.</returns>
        public static Point Add(this Point point, Point other)
        {
            return new Point(point.X + other.X, point.Y + other.Y);
        }

        /// <summary>
        /// Get the Euclidean distance to another point, as a new Point.
        /// </summary>
        /// <param name="other">The Point to compare distance against</param>
        /// <returns>The Euclidean distance between this Point and the other Point.</returns>
        public static double DistanceTo(this Point point, Point other)
        {
            return Math.Sqrt(Math.Pow(other.X - point.X, 2) + Math.Pow(other.Y - point.Y, 2));
        }
    }
}
