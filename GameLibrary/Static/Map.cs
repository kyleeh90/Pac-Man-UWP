using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;

namespace GameLibrary
{
    /// <summary>
    /// Static class for the game map.
    /// </summary>
    public static class Map
    {
        #region Fields

        /// <summary>
        /// Stores whether the map has been initialized.
        /// </summary>
        public static bool isInitialized = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Stores the position and type of every tile
        /// </summary>
        public static Dictionary<Point, TileType> Tiles { get; private set; } = new Dictionary<Point, TileType>();

        #endregion Properties

        #region Initializer

        /// <summary>
        /// Initializes the map data.
        /// </summary>
        public static void Initialize()
        {
            // Only initialize the map once
            if (!isInitialized)
            {
                // Load the tiles
                LoadTiles();

                // Set the flag
                isInitialized = true;
            }
        }

        #endregion Initializer

        #region Public Methods

        /// <summary>
        /// Gets the next tile in a direction from a position.
        /// </summary>
        /// <param name="currentDirection">The direction the object is facing.</param>
        /// <param name="currentPosition">The position of the object.</param>
        /// <returns>The TileType if found, otherwise TileType.Disabled</returns>
        public static TileType GetNextTile(Direction currentDirection, Point currentPosition)
        {
            return GetTileAt(currentDirection.ToPoint().Add(currentPosition));
        }

        /// <summary>
        /// Get the tile type at a specific position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>The TileType at the specified Point, otherwise TileType.Disabled</returns>
        public static TileType GetTileAt(Point position)
        {
            // Check if the tile exists
            if (Tiles.ContainsKey(position))
            {
                return Tiles[position];
            }

            return TileType.Disabled;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the tile data into the Tiles dictionary.
        /// </summary>
        private async static void LoadTiles()
        {
            // Get the Assets folder
            StorageFolder installDirectory = Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await installDirectory.GetFolderAsync("Assets");

            // Get the map file
            StorageFile mapFile = await assetsFolder.GetFileAsync("tiles.txt");

            // Get the lines
            IList<string> mapLines = await FileIO.ReadLinesAsync(mapFile);

            // Loop through the lines
            // Each character is equal to a number from TileType
            // There's 3 padding tiles per side because of the teleporters.
            // Negate the padding to get actual position
            int lineNum = 0;

            foreach (string line in mapLines)
            {
                // Skip the line if it starts with a comment
                if (line.StartsWith("/"))
                {
                    continue;
                }

                // Loop through the characters
                for (int i = 0; i < line.Length; i++)
                {
                    switch (line[i])
                    {
                        case '0':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.Disabled);
                            break;
                        case '1':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.Floor);
                            break;
                        case '2':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.HomeDoor);
                            break;
                        case '3':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.HomeInterior);
                            break;
                        case '4':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.Intersection);
                            break;
                        case '5':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.Restricted);
                            break;
                        case '6':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.Teleport);
                            break;
                        case '7':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.Tunnel);
                            break;
                        case '8':
                            Tiles.Add(new Point(i - 3, lineNum), TileType.Wall);
                            break;
                    }
                }

                // Increment the line number
                lineNum++;
            }
        }

        #endregion Private Methods
    }
}
