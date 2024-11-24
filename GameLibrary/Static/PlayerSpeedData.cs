namespace GameLibrary
{
    /// <summary>
    /// Helper class for player speed data.
    /// </summary>
    public static class PlayerSpeedData
    {
        /// <summary>
        /// An Tuple containing the speed of the player for each level.
        /// </summary>
        /// <returns>A Tuple of (uint, uint) which contains the base speed and speed when ghosts are frightened, respectively.</returns>
        public static (uint, uint) GetSpeedData()
        {
            if (GameManager.CurrentLevel == 1)
            {
                // 80%, 90%
                return (1431655765, 3580548458);
            }
            else if (GameManager.CurrentLevel <= 4) 
            {
                // 90%, 95%
                return (3580548458, 3596266933);
            }
            else if (GameManager.CurrentLevel <= 20)
            {
                // 100%, 100%
                return (1835887981, 1835887981);
            }
            else
            {
                // 90%, 90%
                return (3580548458, 3580548458);
            }
        }
    }
}
