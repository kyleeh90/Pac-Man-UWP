using System;

namespace GameLibrary
{
    /// <summary>
    /// Helper class to keep track of the speed of the ghosts.
    /// </summary>
    public static class GhostSpeedData
    {
        /// <summary>
        /// An array containing the speed of the ghosts for each level.
        /// </summary>
        /// <remarks>
        /// Returns an array with the format: Base speed, tunnel speed, frightened speed.
        /// </remarks>
        /// <returns>An array of uint with all the ghost speeds for the current level.</returns>
        public static uint[] GetSpeedData() 
        {
            if (GameManager.CurrentLevel == 1)
            {
                // 75%, 40%, 50%
                return new uint[] {
                    2854901077,
                    572662306,
                    2451870244,
                };
            }
            else if (GameManager.CurrentLevel <= 4) 
            {
                // 85%, 45%, 55%
                return new uint[] {
                    2859095509,
                    1210327697,
                    2451842377,
                };
                
            }
            else if (GameManager.CurrentLevel <= 20)
            {
                // 95%, 50%, 60%
                return new uint[] {
                    3596266933,
                    2451870244,
                    623191333,
                };
            }
            else
            {
                // 95%, 50%, 95%
                return new uint[] {
                    3596266933,
                    2451870244,
                    3596266933,
                };
            }
        }
    }
}
