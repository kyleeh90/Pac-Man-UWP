using Microsoft.Graphics.Canvas.Text;
using Windows.Foundation;

namespace GameLibrary
{
    public static class Constants
    {
        /// <summary>
        /// The height of the game, in pixels.
        /// </summary>
        public const int HEIGHT = 288;

        /// <summary>
        /// The x-coordinate of the house exit in pixels.
        /// </summary>
        public const int HOUSE_EXIT_X = 114;

        /// <summary>
        /// The y-coordinate of the house exit in pixels.
        /// </summary>
        public const int HOUSE_EXIT_Y = 119;

        /// <summary>
        /// How many pellets are in the game at the start.
        /// </summary>
        public const int STARTING_PELLET_COUNT = 244;

        /// <summary>
        /// The CanvasTextFormat for the game.
        /// </summary>
        /// <remarks>Use for any DrawText methods.</remarks>
        public static CanvasTextFormat TEXT_FORMAT = new CanvasTextFormat() 
        {
            FontFamily = "Assets/PressStart2P.ttf#Press Start 2P",
            FontSize = 8
        };

        /// <summary>
        /// The center of a tile.
        /// </summary>
        public static Point TILE_CENTER = new Point(3, 4);

        /// <summary>
        /// Tile size of the game, in pixels.
        /// </summary>
        public const int TILE_SIZE = 8;

        /// <summary>
        /// The width of the game, in pixels.
        /// </summary>
        public const int WIDTH = 224;
    }
}
