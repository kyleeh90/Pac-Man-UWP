using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    /// <summary>
    /// Helper class to keep track of the time that the ghosts are frightened and how many times they switch from blue to white.
    /// </summary>
    public static class GhostFrightenedData
    {
        // 1 seconds = 60 frames

        /// <summary>
        /// How long the frightened mode lasts (in frames) and when to start flashing (in frames).
        /// </summary>
        /// <returns>A Tuple of (int, int) which contains how many frames to stay frightened and when flashing begins, respectively.</returns>
        public static (int, int) GetFrightenedData()
        {
            switch (GameManager.CurrentLevel) 
            {
                case 1:
                    return (360, 70);
                case 2:
                case 6:
                case 10:
                    return (300, 70);
                case 3:
                    return (240, 70);
                case 4:
                case 14:
                    return (180, 70);
                case 5:
                case 7:
                case 8:
                case 11:
                    return (120, 70);
                case 9:
                case 12:
                case 13:
                case 15:
                case 16:
                case 18:
                    return (60, 42);
                default:
                    return (0, 0);
            }
        }
    }
}
