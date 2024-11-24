namespace GameLibrary
{
    /// <summary>
    /// Helper class that contains the times for each state of the ghosts.
    /// </summary>
    public static class GhostStateTimes
    {
        // 1 seconds = 60 frames

        /// <summary>
        /// The time in frames elapsed for each state of the ghosts.
        /// </summary>
        /// <returns>An int[] containing every state time, in frames.</returns>
        public static int[] GetStateTimes()
        {
            if (GameManager.CurrentLevel == 1)
            {
                return new int[7]
                {
                    420,
                    1200,
                    420,
                    1200,
                    300,
                    1200,
                    300
                };
            }
            else if (GameManager.CurrentLevel <= 4)
            {
                return new int[7]
                {
                    420,
                    1200,
                    420,
                    1200,
                    300,
                    61980,
                    1
                };
            }
            else 
            {
                return new int[7]
                {
                    420,
                    1200,
                    300,
                    1200,
                    300,
                    62040,
                    1
                };
            }
        }
    }
}
