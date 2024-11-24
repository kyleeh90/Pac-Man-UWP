using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace GameLibrary
{
    /// <summary>
    /// Manager class to handle score, lives, levels, etc.
    /// </summary>
    public static class GameManager
    {
        private static int _coins = 0, _currentScore = 0, _displayCurrentScore = 0, _loadedHighScore = 0;

        /// <summary>
        /// How many coins are in the machine.
        /// </summary>
        public static int Coins
        {
            get => _coins;
            set 
            {
                if (value > 9) 
                {
                    _coins = 9;
                }
                else if (value < 0)
                {
                    _coins = 0;
                }
                else
                {
                    _coins = value;
                }
            }
        }

        /// <summary>
        /// The current level the player is on.
        /// </summary>
        public static int CurrentLevel { get; set; } = 1;

        // Some preamble that applies to the score and high score. A perfect score of Pac-Man is 3,333,360 points.
        // The display only shows 6 digits for each however. The high score doesn't show the last 6 digits however of the score.
        // It shows the last 6 digits of the highest score. E.g. 1,999,000 would be shown as 999000.
        // And while technically when you have 2,100,000 that is greater than 1,999,000 obviously, the high score would display as 999000.

        /// <summary>
        /// The player's current score.
        /// </summary>
        public static int CurrentScore 
        {
            get => _currentScore;
            set 
            {
                _currentScore = value;

                if (_currentScore > HighScore)
                {
                    HighScore = _currentScore;
                }

                DisplayCurrentScore = value;
            }
        }

        /// <summary>
        /// The current score to actually display.
        /// </summary>
        public static int DisplayCurrentScore 
        {
            get => _displayCurrentScore;
            private set 
            {
                // Remove the millions place
                _displayCurrentScore = value % 1000000;

                if (_displayCurrentScore > DisplayHighScore)
                {
                    DisplayHighScore = _displayCurrentScore;
                }
            } 
        }

        /// <summary>
        /// The high score of the machine.
        /// </summary>
        public static int HighScore { get; private set; } = 0;

        public static bool IsNewHighScore => HighScore > _loadedHighScore;

        /// <summary>
        /// The high score to actually display.
        /// </summary>
        public static int DisplayHighScore { get; private set; } = 0;

        /// <summary>
        /// The amount of lives the player has left.
        /// </summary>
        public static int LivesRemaining { get; set; } = 2;

        /// <summary>
        /// Loads the high score from the file.
        /// </summary>
        public static async Task LoadHighScore() 
        {
            // Get the AppData folder
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            // Get the file if it exists
            StorageFile storageFile = await storageFolder.TryGetItemAsync("HighScore.txt") as StorageFile;

            // If the file exists, read the high score from it
            if (storageFile != null)
            {
                string highScoreString = await FileIO.ReadTextAsync(storageFile);

                // Temporarily set the current score to the high score
                CurrentScore = int.Parse(highScoreString);
                CurrentScore = 0;

                _loadedHighScore = int.Parse(highScoreString);
            }
        }

        /// <summary>
        /// Saves the high score to the file.
        /// </summary>
        /// <returns>True if it's a new high score- false otherwise.</returns>
        public static async Task SaveHighScore() 
        {
            // Get the AppData folder
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            // Get the file if it exists
            StorageFile storageFile = await storageFolder.TryGetItemAsync("HighScore.txt") as StorageFile;

            // If the file exists, read the high score from it
            if (storageFile != null)
            {
                string highScoreString = await FileIO.ReadTextAsync(storageFile);

                // Parse the score
                int highScore = int.Parse(highScoreString);

                // Check if it's higher
                if (CurrentScore > highScore)
                {
                    // Change the loaded score
                    _loadedHighScore = CurrentScore;

                    // Write the new high score
                    await FileIO.WriteTextAsync(storageFile, HighScore.ToString());
                }
            }
            else 
            {
                storageFile = await storageFolder.CreateFileAsync("HighScore.txt");

                // Write the new high score
                await FileIO.WriteTextAsync(storageFile, HighScore.ToString());
            }
        }

        /// <summary>
        /// Resets the game to its initial state.
        /// </summary>
        public static void Reset() 
        {
            CurrentScore = 0;
            LivesRemaining = 2;
            CurrentLevel = 1;
        }
    }
}
