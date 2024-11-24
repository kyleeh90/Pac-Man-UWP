using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using GameLibrary;
using Windows.UI.Xaml;
using System.Diagnostics;
using System;
using System.Threading.Tasks;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

/* UWP Game Template
 * Created By: Melissa VanderLely
 * Modified By: Kyle Henderson
 */


namespace GameInterface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage() 
        {
            // Set the window starting size and minimum size
            ApplicationView.PreferredLaunchViewSize = new Size(224, 288);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(224, 288));

            InitializeComponent();

            // Initialize the map
            Map.Initialize();

            // Load the high score
            LoadHighScore();
        }

        private async Task LoadHighScore() 
        {
            // Load the high score
            await GameManager.LoadHighScore();

            // Add the attract mode
            AttractMode attractMode = new AttractMode();
            attractMode.GameStarted += onAttractMode_GameStarted;

            gridMain.Children.Add(attractMode);
        }

        private void onAttractMode_GameStarted(object sender, EventArgs e) 
        {
            // Unhook the event
            (sender as AttractMode).GameStarted -= onAttractMode_GameStarted;

            // Remove the attract mode
            gridMain.Children.Clear();

            // Add the first level
            GameLevel firstLevel = new GameLevel();
            gridMain.Children.Add(firstLevel);

            // Hook the new level into the event
            firstLevel.LevelComplete += onGameLevel_LevelComplete;
            firstLevel.GameOver += onGameLevel_GameOver;
        }

        private void onGameLevel_GameOver(object sender, EventArgs e)
        {
            // Unhook the event
            (sender as GameLevel).GameOver -= onGameLevel_GameOver;

            // Remove the current level
            gridMain.Children.Clear();

            // Add the attract mode back to the grid
            AttractMode attractMode = new AttractMode();
            attractMode.GameStarted += onAttractMode_GameStarted;

            gridMain.Children.Add(attractMode);
        }

        private void onGameLevel_LevelComplete(object sender, EventArgs e)
        {
            // Unhook the event
            (sender as GameLevel).LevelComplete -= onGameLevel_LevelComplete;
            (sender as GameLevel).GameOver -= onGameLevel_GameOver;

            GameManager.CurrentLevel++;

            // Remove the current level
            gridMain.Children.Clear();

            // Add the next level
            GameLevel nextLevel = new GameLevel();
            gridMain.Children.Add(nextLevel);

            // Hook the new level into the event
            nextLevel.LevelComplete += onGameLevel_LevelComplete;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Get the target height
            RenderProperties.TargetHeight = e.NewSize.Height;

            // Get the new window size
            RenderProperties.WindowHeight = e.NewSize.Height;
            RenderProperties.WindowWidth = e.NewSize.Width;
        }
    }
}
