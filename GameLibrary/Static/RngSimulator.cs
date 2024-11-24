using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    public static class RngSimulator
    {
        /// <summary>
        /// Instance of Random for getting random doubles.
        /// </summary>
        /// <remarks>Seeded to 0 for deterministic results.</remarks>
        private static Random _rng = new Random(0);

        // Create a dictionary to store the directions and their probabilities
        // Numbers based on analysis of a ROM disassembly of the original game.
        /// <summary>
        /// Dictionary of directions and their probabilities.
        /// </summary>
        private static Dictionary<Direction, double> _directions = new Dictionary<Direction, double>
        {
            { Direction.Up, 0.164 },
            { Direction.Left, 0.252 },
            { Direction.Down, 0.285 },
            { Direction.Right, 0.299 }
        };

        /// <summary>
        /// Gets a random direction for a frightened ghost.
        /// </summary>
        /// <returns>A random Direction</returns>
        public static Direction GetDirection() 
        {
            // Get a random double
            double start = _rng.NextDouble();

            // Loop through the directions
            foreach (KeyValuePair<Direction, double> entry in _directions) 
            {
                // If the number is less than the probability of the direction, return it
                if (start < entry.Value)
                {
                    return entry.Key;
                }

                // Subtract the probability of the direction from the random double
                start -= entry.Value;
            }

            // Shouldn't reach here, but return none if it does
            return Direction.None;
        }
    }
}
