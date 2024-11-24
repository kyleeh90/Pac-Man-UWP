using GameLibrary;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GameInterface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AttractMode : Page
    {
        #region Events

        public EventHandler GameStarted;

        #endregion Events

        #region Fields

        // A bool used to blink the power pellet
        private bool powerPelletVisible = true;

        // CanvasBitmaps for everything that needs to be loaded
        // The pellets
        private CanvasBitmap pelletSprite, powerPelletSprite;

        // The ghosts
        private CanvasBitmap blinkySprite, pinkySprite, inkySprite, clydeSprite;

        // The shared CanvasDevice for drawing
        readonly CanvasDevice device = CanvasDevice.GetSharedDevice();

        // The amount of updates that have been done
        private long updateCount = 0;

        #endregion Fields

        #region Constructors

        public AttractMode()
        {
            InitializeComponent();

            Window.Current.CoreWindow.KeyDown += coreWindow_KeyDown;
        }

        #endregion Constructors

        #region Draw Methods

        private void canvasAnimated_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            // Update the target size
            RenderProperties.UpdateTargetSize();

            // Draw the intro screen
            CanvasRenderTarget offscreen = new CanvasRenderTarget(device, Constants.WIDTH, Constants.HEIGHT, 96);
            using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
            {
                #region Always Draw

                ds.Clear(Colors.Black);

                ds.DrawText(
                    "1UP",
                    24, 0,
                    Colors.White,
                    Constants.TEXT_FORMAT
                );

                ds.DrawText(
                    GameManager.DisplayCurrentScore == 0 ? "00" : $"{GameManager.DisplayCurrentScore}",
                    GameManager.DisplayCurrentScore == 0 ? 42 : 56 - (GameManager.DisplayCurrentScore.ToString().Length * Constants.TILE_SIZE),
                    10,
                    Colors.White,
                    Constants.TEXT_FORMAT
                );

                ds.DrawText(
                    "HIGH SCORE",
                    76, 0,
                    Colors.White,
                    Constants.TEXT_FORMAT
                );

                ds.DrawText(
                    GameManager.DisplayHighScore == 0 ? ""
                    : $"{GameManager.DisplayHighScore}",
                    140 - (GameManager.DisplayHighScore.ToString().Length * Constants.TILE_SIZE),
                    10,
                    Colors.White,
                    Constants.TEXT_FORMAT
                );

                ds.DrawText(
                    "2UP",
                    174, 0,
                    Colors.White,
                    Constants.TEXT_FORMAT
                );

                ds.DrawText(
                    "CHARACTER / NICKNAME",
                    56, 40,
                    Colors.White,
                    Constants.TEXT_FORMAT
                );

                ds.DrawText(
                    $"CREDIT  {GameManager.Coins}",
                    17, 280,
                    Colors.White,
                    Constants.TEXT_FORMAT
                );

                #endregion Always Draw

                #region Blinky Draw

                // TODO: Put a cap on how long it's drawn
                if (updateCount >= 60)
                    ds.DrawImage(blinkySprite, new Rect(34, 52, 14, 14));

                if (updateCount >= 110)
                    ds.DrawText(
                        "-SHADOW",
                        55, 56,
                        Colors.Red,
                        Constants.TEXT_FORMAT
                    );

                if (updateCount >= 145)
                    ds.DrawText(
                        "”BLINKY”",
                        144, 56,
                        Colors.Red,
                        Constants.TEXT_FORMAT
                    );

                #endregion Blinky Draw

                #region Pinky Draw

                // TODO: Put a cap on how long it's drawn
                if (updateCount >= 170)
                    ds.DrawImage(pinkySprite, new Rect(34, 75, 14, 14));

                if (updateCount >= 220)
                    ds.DrawText(
                        "-SPEEDY",
                        55, 79,
                        Color.FromArgb(255, 255, 183, 255),
                        Constants.TEXT_FORMAT
                    );

                if (updateCount >= 255)
                    ds.DrawText(
                        "”PINKY”",
                        144, 79,
                        Color.FromArgb(255, 255, 183, 255),
                        Constants.TEXT_FORMAT
                    );

                #endregion Pinky Draw

                #region Inky Draw

                // TODO: Put a cap on how long it's drawn
                if (updateCount >= 280)
                    ds.DrawImage(inkySprite, new Rect(34, 98, 14, 14));

                if (updateCount >= 330)
                    ds.DrawText(
                        "-BASHFUL",
                        55, 102,
                        Colors.Cyan,
                        Constants.TEXT_FORMAT
                    );

                if (updateCount >= 365)
                    ds.DrawText(
                        "”INKY”",
                        144, 102,
                        Colors.Cyan,
                        Constants.TEXT_FORMAT
                    );

                #endregion Inky Draw

                #region Clyde Draw

                // TODO: Put a cap on how long it's drawn
                if (updateCount >= 390)
                    ds.DrawImage(clydeSprite, new Rect(34, 121, 14, 14));

                if (updateCount >= 440)
                    ds.DrawText(
                        "-POKEY",
                        55, 125,
                        Color.FromArgb(255, 255, 183, 81),
                        Constants.TEXT_FORMAT
                    );

                if (updateCount >= 475)
                    ds.DrawText(
                        "”CLYDE”",
                        144, 125,
                        Color.FromArgb(255, 255, 183, 81),
                        Constants.TEXT_FORMAT
                    );

                #endregion Clyde Draw

                #region Pellet Draw

                if (updateCount >= 560)
                {
                    ds.DrawImage(pelletSprite, new Rect(76, 190, 8, 8));
                    ds.DrawText(
                        "10 PTS",
                        92, 190,
                        Colors.White,
                        Constants.TEXT_FORMAT
                    );

                    if (powerPelletVisible)
                    {
                        ds.DrawImage(powerPelletSprite, new Rect(76, 205, 8, 8));
                    }

                    ds.DrawText(
                        "50 PTS",
                        92, 205,
                        Colors.White,
                        Constants.TEXT_FORMAT
                    );
                }

                #endregion Pellet Draw

                #region Copyright Draw

                if (updateCount >= 620)
                {
                    ds.DrawText(
                        "© 1980 MIDWAY MFG.CO.",
                        30, 246,
                        Color.FromArgb(255, 255, 183, 255),
                        Constants.TEXT_FORMAT
                    );
                }

                #endregion Copyright Draw
            }

            // Draw the offscreen image to the screen
            args.DrawingSession.DrawImage(
                offscreen,
                new Rect(
                    RenderProperties.CenterX,
                    RenderProperties.CenterY,
                    RenderProperties.TargetWidth,
                    RenderProperties.TargetHeight
                ),
                new Rect(0, 0, Constants.WIDTH, Constants.HEIGHT),
                1,
                CanvasImageInterpolation.NearestNeighbor
            );
        }

        #endregion Draw Methods

        #region Create Resource Methods

        private void canvasAnimated_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            // Load the images
            args.TrackAsyncAction(LoadImages(sender).AsAsyncAction());
        }

        /// <summary>
        /// A method to load the images of the intro.
        /// </summary>
        /// <param name="sender">The CanvasControl or AnimatedCanvasControl that is calling this method</param>
        private async Task LoadImages(ICanvasResourceCreator sender)
        {
            // Load the ghosts
            blinkySprite = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Blinky/blinky_right.png"));
            pinkySprite = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Pinky/pinky_right.png"));
            inkySprite = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Inky/inky_right.png"));
            clydeSprite = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Clyde/clyde_right.png"));

            // Load the pellets
            pelletSprite = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pellets/pellet.png"));
            powerPelletSprite = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pellets/power_pellet.png"));
        }

        #endregion Create Resource Methods

        #region Update Methods

        private void canvasAnimated_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            // Increment the update count
            updateCount++;

            // Force a redraw at certain points
            switch (updateCount)
            {
                case 60:
                case 110:
                case 145:
                case 170:
                case 220:
                case 255:
                case 280:
                case 330:
                case 365:
                case 390:
                case 440:
                case 475:
                case 560:
                case 620:
                    canvasAnimated.Invalidate();
                    break;
            }

            // Blink the power pellet every 20 frames
            if (updateCount >= 620 && updateCount % 20 == 0)
            {
                powerPelletVisible = !powerPelletVisible;
                canvasAnimated.Invalidate();
            }
        }

        #endregion Update Methods

        #region Input Methods

        private async void coreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            // Handle starting the game and adding coins
            switch (e.VirtualKey)
            {
                case VirtualKey.LeftShift:
                case VirtualKey.RightShift:
                case VirtualKey.Shift:
                    GameManager.Coins++;
                    break;
                case VirtualKey.Enter:
                    // Only start the game if there are coins
                    if (GameManager.Coins > 0) 
                    {
                        GameManager.Coins--;

                        GameManager.Reset();

                        await Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, () =>
                            GameStarted?.Invoke(this, new EventArgs())
                        );
                    }
                    break;
            }
        }

        #endregion Input Methods

        // Needed to prevent memory leaks
        private void canvasAnimated_Unloaded(object sender, RoutedEventArgs e)
        {
            canvasAnimated.RemoveFromVisualTree();
            canvasAnimated = null;

            Window.Current.CoreWindow.KeyDown -= coreWindow_KeyDown;
        }
    }
}
