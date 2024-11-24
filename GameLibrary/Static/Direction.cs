using System;
using Windows.Foundation;

namespace GameLibrary
{
    // When ghosts have a tie for the shortest path, they will choose the direction in this order
    //  up, left, down, right. The enum is the same order so the lowest number is always preferred

    /// <summary>
    /// Contains the different directions the player can move.
    /// </summary>
    /// <remarks>Direction.None is last to maintain order</remarks>
    public enum Direction
    {
        Up,
        Left,
        Down,
        Right,
        None
    }

    /// <summary>
    /// Extension methods for the Direction enum.
    /// </summary>
    public static class DirectionExtensions
    {
        /// <summary>
        /// Determines if the Direction is horizontal.
        /// </summary>
        /// <returns>True if the Direction is horizontal, false otherwise.</returns>
        public static bool IsHorizontal(this Direction direction)
        {
            return direction == Direction.Left || direction == Direction.Right;
        }

        /// <summary>
        /// Provides the opposite of a direction.
        /// </summary>
        /// <remarks>Mostly used for ghost AI as they can't turn around under normal circumstances.</remarks>
        /// <returns>The opposite of this Direction</returns>
        public static Direction Reversed(this Direction direction) 
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Right:
                    return Direction.Left;
                default:
                    return Direction.None;
            }
        }

        /// <summary>
        /// Provides the direction that is 90 degrees clockwise from this Direction.
        /// </summary>
        /// <returns>This Direction rotated by 90 degrees clockwise.</returns>
        public static Direction RotatedClockwise(this Direction direction) 
        {
            switch (direction) 
            {
                case Direction.Up:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Left;
                case Direction.Left:
                    return Direction.Up;
                default:
                    return Direction.None;
            }
        }

        /// <summary>
        /// Converts a Direction to a Point.
        /// </summary>
        /// <returns>This Direction as a Point or Point(0,0) for Direction.None</returns>
        public static Point ToPoint(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Point(0, -1);
                case Direction.Left:
                    return new Point(-1, 0);
                case Direction.Down:
                    return new Point(0, 1);
                case Direction.Right:
                    return new Point(1, 0);
                default:
                    return new Point(0, 0);
            }
        }
    }
}
