using GameLibrary;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GameInterface
{
    /// <summary>
    /// An enum representing the state of the game.
    /// </summary>
    public enum GameState
    {
        Complete,
        Idle,
        Intro,
        PlayerDeath,
        Playing
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameLevel : Page
    {
        #region Events

        public EventHandler GameOver, LevelComplete;

        #endregion Events

        #region Constants - Dots Remaining For Fruit

        private const int DOTS_REMAINING_FIRST = 244 - 70;
        private const int DOTS_REMAINING_SECOND = 244 - 170;

        #endregion Constants - Dots Remaining For Fruit

        #region Fields - Audio

        // MediaPlayer for level intro music
        private MediaPlayer introPlayer = new MediaPlayer();

        // MediaPlayer for fruit eaten sound effect
        private MediaPlayer fruitPlayer = new MediaPlayer();

        #endregion Fields - Audio

        #region Fields - Entities

        // The maze object
        private Entity maze = new Entity(0, 25, new Size(Constants.WIDTH, Constants.HEIGHT - 40));

        // Pellets, in two different lists as they have different logic and it makes more sense to me than checking
        // the type of pellet while iterating through a list of simply Pellet.
        private List<Pellet> pellets = new List<Pellet>();
        private List<PowerPellet> powerPellets = new List<PowerPellet>();

        // List to contain the ghosts
        private List<Ghost> ghosts = new List<Ghost>();

        // The player object
        private Player player = new Player(
            14 * Constants.TILE_SIZE, 
            26 * Constants.TILE_SIZE + (int)Constants.TILE_CENTER.Y,
            new Size(13, 13)
        );

        #endregion Fields - Entities

        #region Fields - Rendering

        //A shared CanvasDevice for drawing on the screen.
        private CanvasDevice device = CanvasDevice.GetSharedDevice();

        // Sprite for the player lives in the bottom left
        private CanvasBitmap playerLivesSprite;

        // Sprites for the end of level animation
        private CanvasBitmap[] endSprites = new CanvasBitmap[2];

        // Whether the 1UP text should be visible
        private bool oneUpVisible = true;

        #endregion Fields - Rendering

        #region Fields - Timing

        /// <summary>
        /// The frame count when the level is complete or when the player dies.
        /// </summary>
        private long levelStoppedFrame = -1;

        // A timer which resets when the player eats a pellet. When it reachers 0, a ghost is released from the house if there are any left
        // The timer starts at 240 frames (4 seconds) and then on/after level 5 it's 180 frames (3 seconds)
        private readonly int pelletTimerStart = GameManager.CurrentLevel >= 5 ? 180 : 240;
        private int pelletTimer = 0;

        // Keeps track of whether a pellet was eaten this frame
        private bool pelletEaten = false;

        #endregion Fields - Timing

        #region Fields - Other

        private bool isNewHighScore = false;

        private GameState currentState = GameState.Intro;

        #endregion Fields - Other

        #region Properties - Static

        /// <summary>
        /// The frightened audio player.
        /// </summary>
        private static MediaElement FrightenedPlayer { get; set; }

        #endregion Properties - Static

        #region Constructors

        public GameLevel()
        {
            InitializeComponent();

            // Initialize the ghost data
            Ghost.InitializeData();

            // Create the ghosts
            ghosts.Add(new Blinky(
                14 * Constants.TILE_SIZE + 2,
                15 * Constants.TILE_SIZE - 1,
                new Size(14, 14))
            );

            ghosts.Add(new Clyde(
                16 * Constants.TILE_SIZE + 2,
                18 * Constants.TILE_SIZE - 1,
                new Size(14, 14))
            );

            ghosts.Add(new Inky(
                12 * Constants.TILE_SIZE + 2,
                18 * Constants.TILE_SIZE - 1,
                new Size(14, 14))
            );

            ghosts.Add(new Pinky(
                14 * Constants.TILE_SIZE + 2,
                18 * Constants.TILE_SIZE - 1,
                new Size(14, 14))
            );

            // Set volumes
            fruitPlayer.Volume = 0.05;

            // The intro player only plays the first time on the first level
            introPlayer.Volume = GameManager.CurrentLevel > 1 ? 0 : 0.05;

            // Add the input event handlers
            Window.Current.CoreWindow.KeyDown += coreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += coreWindow_KeyUp;

            // Load the sound effects
            fruitPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Sounds/eat_fruit.wav"));

            // Start the intro music
            introPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Sounds/level_intro.wav"));
            introPlayer.Play();

            // Start player and ghosts after the intro music
            introPlayer.MediaEnded += (s, e) =>
            {
                // Start the game
                currentState = GameState.Playing;
                canvasStatic.Invalidate();

                // Starts the ghosts
                ghosts[0].StateMachine.SetState(GhostStateType.Scatter);
                ghosts[1].StateMachine.SetState(GhostStateType.Home);
                ghosts[2].StateMachine.SetState(GhostStateType.Home);
                ghosts[3].StateMachine.SetState(GhostStateType.Home);

                // Start the player
                player.SetState(PlayerState.Moving);

                pelletTimer = pelletTimerStart;
            };
        }

        #endregion Constructors

        #region Create Resources - Animated

        private void canvasAnimated_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            // Load the animated resources
            args.TrackAsyncAction(LoadAnimated(sender).AsAsyncAction());
        }

        private async Task LoadAnimated(ICanvasResourceCreator sender)
        {
            // Load the power pellets
            await LoadPowerPellets(sender);

            // Load the ghost resources
            await Ghost.LoadSharedResources(sender);
            await ((Blinky)ghosts[0]).LoadResources(sender);
            await ((Clyde)ghosts[1]).LoadResources(sender);
            await ((Inky)ghosts[2]).LoadResources(sender);
            await ((Pinky)ghosts[3]).LoadResources(sender);

            // Load the ghost score sprites
            await GhostDead.LoadResources(sender);

            // Load the player sprites
            await player.LoadResources(sender);
        }

        #endregion Create Resources - Animated

        #region Create Resources - Static

        private void canvasStatic_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            // Load the static resources
            args.TrackAsyncAction(LoadStatic(sender).AsAsyncAction());
        }

        private async Task LoadStatic(ICanvasResourceCreator sender) 
        {
            // Load the maze sprite
            maze.Sprite = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Maze/maze.png"));

            // Load the player lives sprite
            playerLivesSprite = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/life_counter.png"));

            // Load the end sprites
            endSprites[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Maze/maze_clear_white.png"));
            endSprites[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Maze/maze_clear_blue.png"));

            // Load the pellets
            await LoadPellets(sender);
        }

        #endregion Create Resources - Static

        #region Draw - Animated

        private void canvasAnimated_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            // Update the target size before drawing
            RenderProperties.UpdateTargetSize();

            // Draw the animated images offscreen
            CanvasRenderTarget offscreen = new CanvasRenderTarget(device, Constants.WIDTH, Constants.HEIGHT, 96);
            using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
            {
                // Clear the screen before drawing
                ds.Clear(Colors.Transparent);

                // Draw the power pellets
                if (powerPellets.Count > 0)
                {
                    for (int i = 0; i < powerPellets.Count; i++)
                    {
                        powerPellets[i].Draw(ds);
                    }
                }

                // Draw the player and ghosts
                // Draw the player below if not energized, otherwise on top
                // Also draw the ghosts as (position in array in square brackets)
                // Clyde[1], Inky[2], Pinky[3], Blinky[0] 
                if (!player.IsEnergized)
                {
                    player.Draw(ds);

                    for (int i = 1; i <= 3; i++)
                    {
                        if (ghosts[i].Visible)
                        {
                            ghosts[i].Draw(ds);
                        }
                    }

                    if (ghosts[0].Visible)
                    {
                        ghosts[0].Draw(ds);
                    }
                }
                else 
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        if (ghosts[i].Visible)
                        {
                            ghosts[i].Draw(ds);
                        }
                    }

                    if (ghosts[0].Visible)
                    {
                        ghosts[0].Draw(ds);
                    }

                    player.Draw(ds);
                }

                // Draw the 1UP text if it's visible
                // The 1UP text blinks every ~30 frames
                if (oneUpVisible)
                {
                    ds.DrawText(
                        "1UP",
                        24, 0,
                        Colors.White,
                        Constants.TEXT_FORMAT
                    );
                }
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

        #endregion Draw - Animated

        #region Draw - Static

        private void canvasStatic_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            // Update the target size before drawing
            RenderProperties.UpdateTargetSize();

            // Draw the static images offscreen
            CanvasRenderTarget offscreen = new CanvasRenderTarget(device, Constants.WIDTH, Constants.HEIGHT, 96);
            using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
            {
                // Clear the screen before drawing
                ds.Clear(Colors.Black);

                // Draw the maze
                maze.Draw(ds);

                // Draw the pellets
                if (pellets.Count > 0)
                {
                    for (int i = 0; i < pellets.Count; i++)
                    {
                        pellets[i].Draw(ds);
                    }
                }

                #region Draw Scores

                // Draw the score
                // Can't pad the string with spaces to position as it needs a pixel location to draw
                ds.DrawText(
                    GameManager.DisplayCurrentScore == 0 ? "00" : $"{GameManager.DisplayCurrentScore}",
                    GameManager.DisplayCurrentScore == 0 ? 42 : 56 - (GameManager.DisplayCurrentScore.ToString().Length * Constants.TILE_SIZE),
                    10,
                    Colors.White,
                    Constants.TEXT_FORMAT
                );

                // Draw the high score
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

                #endregion Draw Scores

                #region Draw Ready

                if (currentState == GameState.Intro) 
                {
                    // Draw the ready message
                    ds.DrawText(
                        "READY!",
                        90, 162,
                        Colors.Yellow,
                        Constants.TEXT_FORMAT
                    );
                }

                #endregion Draw Ready

                // Draw the player lives
                for (int i = 1; i <= GameManager.LivesRemaining; i++)
                {
                    ds.DrawImage( playerLivesSprite, new Rect(i * 16, Constants.HEIGHT - 13, 10, 11));
                }
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

        #endregion Draw - Static

        #region Methods - Load Pellets

        /// <summary>
        /// Loads every pellet for the level.
        /// </summary>
        /// <param name="sender">The CanvasControl or AnimatedCanvasControl that is calling this method</param>
        /// <returns></returns>
        private async Task LoadPellets(ICanvasResourceCreator sender)
        {
            // Load pellet images
            CanvasBitmap pelletImage = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pellets/pellet.png"));

            // Get the Assets folder
            StorageFolder installDirectory = Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await installDirectory.GetFolderAsync("Assets");

            // Get the map file
            StorageFile mapFile = await assetsFolder.GetFileAsync("pellets.txt");

            // Get the lines
            IList<string> mapLines = await FileIO.ReadLinesAsync(mapFile);

            // Loop through the lines
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
                        case 'o':
                            pellets.Add(new Pellet(
                                i * Constants.TILE_SIZE,
                                lineNum * Constants.TILE_SIZE,
                                new Size(Constants.TILE_SIZE, Constants.TILE_SIZE))
                            { Sprite = pelletImage }
                            );
                            break;
                    }
                }

                // Increment the line number
                lineNum++;
            }
        }

        /// <summary>
        /// Load every power pellet for the level.
        /// </summary>
        /// <param name="sender">The CanvasControl or AnimatedCanvasControl that is calling this method</param>
        /// <returns></returns>
        private async Task LoadPowerPellets(ICanvasResourceCreator sender)
        {
            // Load pellet image
            CanvasBitmap powerPelletImage = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pellets/power_pellet.png"));

            // Load the power pellets
            powerPellets.Add(new PowerPellet(
                Constants.TILE_SIZE,
                6 * Constants.TILE_SIZE,
                new Size(Constants.TILE_SIZE, Constants.TILE_SIZE))
            { Sprite = powerPelletImage }
            );

            powerPellets.Add(new PowerPellet(
                26 * Constants.TILE_SIZE,
                6 * Constants.TILE_SIZE,
                new Size(Constants.TILE_SIZE, Constants.TILE_SIZE))
            { Sprite = powerPelletImage }
            );

            powerPellets.Add(new PowerPellet(
                Constants.TILE_SIZE,
                26 * Constants.TILE_SIZE,
                new Size(Constants.TILE_SIZE, Constants.TILE_SIZE))
            { Sprite = powerPelletImage }
            );

            powerPellets.Add(new PowerPellet(
                26 * Constants.TILE_SIZE,
                26 * Constants.TILE_SIZE,
                new Size(Constants.TILE_SIZE, Constants.TILE_SIZE))
            { Sprite = powerPelletImage }
            );
        }

        #endregion Methods - Load Pellets

        #region Methods - Collision

        /// <summary>
        /// Check for collision with the ghosts
        /// </summary>
        private void HandleGhostCollision() 
        {
            for (int i = 0; i < 4; i++) 
            {
                HandleGhostCollision(ghosts[i]);
            }
        }

        /// <summary>
        /// Check for collision with the given ghost.
        /// </summary>
        /// <param name="ghost">The ghost to check collision with</param>
        private void HandleGhostCollision(Ghost ghost)
        {
            if (player.GridPosition == ghost.GridPosition && ghost.CurrentState != GhostStateType.Dead)
            {
                // Check if the ghost is frightened
                if (ghost.IsFrightened)
                {
                    // Set the ghost to dead
                    ghost.StateMachine.SetState(GhostStateType.Dead);

                    // Set ghost to not frightened
                    ghost.IsFrightened = false;

                    // Pause the player
                    player.FreezeFramesRemaining += 180;

                    // Check if the freeze time is too high, happens when eating two ghosts simultaneously
                    if (player.FreezeFramesRemaining >=300)
                    {
                        player.FreezeFramesRemaining = 180;
                    }

                    // Pause the ghosts
                    for (int i = 0; i < 4; i++) 
                    {
                        ghosts[i].FreezeFramesRemaining = 180;
                    }

                    // Add the score
                    GameManager.CurrentScore += Player.GhostMultiplier * ghosts[0].Points;
                    canvasStatic.Invalidate();

                    // Increment the multiplier
                    Player.GhostMultiplier *= 2;
                }
                else if (ghost.CurrentState == GhostStateType.Chase || ghost.CurrentState == GhostStateType.Scatter ||
                         ghost.CurrentState == GhostStateType.LeavingHome)
                {
                    currentState = GameState.PlayerDeath;
                    Ghost.GlobalPelletCounter = 0;
                }
            }
        }

        /// <summary>
        /// Checks for collision between the player and the pellets.
        /// </summary>
        private void HandlePelletCollision()
        {
            // Loop through the pellets
            if (pellets.Count > 0)
            {
                for (int i = 0; i < pellets.Count; i++)
                {
                    // Check for collision
                    if (player.GridPosition == pellets[i].GridPosition)
                    {
                        // Increase the score
                        // Can just grab the first element as the score is the same for all pellets
                        GameManager.CurrentScore += pellets[0].Points;

                        // Remove the pellet
                        pellets.SwapRemoveAt(i);

                        // Redraw the canvas
                        canvasStatic.Invalidate();

                        // Freeze the player for 1 frame
                        player.FreezeFramesRemaining += 2;

                        // Set the pellet eaten flag
                        pelletEaten = true;

                        break;
                    }
                }
            }

            // Also check for power pellets
            if (powerPellets.Count > 0)
            {
                for (int i = 0; i < powerPellets.Count; i++)
                {
                    // Check for collision
                    if (player.GridPosition == powerPellets[i].GridPosition)
                    {
                        // Increase the score
                        // Can just grab the first element as the score is the same for all power pellets
                        GameManager.CurrentScore += powerPellets[0].Points;

                        // Remove the power pellet
                        powerPellets.RemoveAt(i);

                        // Redraw the canvas
                        canvasStatic.Invalidate();

                        // Energize the player
                        player.Energize();

                        // Set all the ghosts to frightened
                        for (int j = 0; j < 4; j++) 
                        {
                            ghosts[j].IsFrightened = true;
                        }

                        // Freeze the player for 3 frames
                        player.FreezeFramesRemaining += 6;

                        // Set the pellet eaten flag
                        pelletEaten = true;

                        break;
                    }
                }
            }

            // If there's no pellets remaining, stop the game
            if (pellets.Count == 0 && powerPellets.Count == 0)
            {
                currentState = GameState.Complete;
            }
        }

        #endregion Methods - Collision

        #region Input Methods

        private void coreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            // Switch the player direction
            switch (e.VirtualKey)
            {
                case VirtualKey.A:
                case VirtualKey.Left:
                    if (player.DesiredDirection != Direction.Left)
                    {
                        player.DesiredDirection = Direction.Left;
                    }
                    break;
                case VirtualKey.D:
                case VirtualKey.Right:
                    if (player.DesiredDirection != Direction.Right)
                    {
                        player.DesiredDirection = Direction.Right;
                    }
                    break;
                case VirtualKey.W:
                case VirtualKey.Up:
                    if (player.DesiredDirection != Direction.Up)
                    {
                        player.DesiredDirection = Direction.Up;
                    }
                    break;
                case VirtualKey.S:
                case VirtualKey.Down:
                    if (player.DesiredDirection != Direction.Down)
                    {
                        player.DesiredDirection = Direction.Down;
                    }
                    break;
            }
        }

        private void coreWindow_KeyUp(CoreWindow sender, KeyEventArgs e)
        {
            // Switch the player direction
            switch (e.VirtualKey)
            {
                case VirtualKey.A:
                case VirtualKey.Left:
                    if (player.DesiredDirection == Direction.Left)
                    {
                        player.DesiredDirection = Direction.None;
                    }
                    break;
                case VirtualKey.D:
                case VirtualKey.Right:
                    if (player.DesiredDirection == Direction.Right)
                    {
                        player.DesiredDirection = Direction.None;
                    }
                    break;
                case VirtualKey.W:
                case VirtualKey.Up:
                    if (player.DesiredDirection == Direction.Up)
                    {
                        player.DesiredDirection = Direction.None;
                    }
                    break;
                case VirtualKey.S:
                case VirtualKey.Down:
                    if (player.DesiredDirection == Direction.Down)
                    {
                        player.DesiredDirection = Direction.None;
                    }
                    break;
            }
        }

        #endregion Input Methods

        #region Update Methods

        private async void canvasAnimated_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            #region State - Playing

            if (currentState == GameState.Playing)
            {
                // Divide the delta by 2 and then update twice
                double halfDelta = args.Timing.ElapsedTime.TotalSeconds / 2;

                // Update the player
                // Check ghost collision more often to avoid pass through
                player.Update(halfDelta, args.Timing.UpdateCount);
                player.Update(halfDelta, args.Timing.UpdateCount, true);
                HandleGhostCollision();

                // Handle pellet collision
                HandlePelletCollision();

                // Decrease the pellet timer
                if (pelletTimer > 0)
                {
                    pelletTimer--;
                }
                else if (pelletTimer == 0)
                {
                    // Release a ghost
                    if (ghosts[3].StateMachine.StateType == GhostStateType.Home)
                    {
                        ghosts[3].StateMachine.SetState(GhostStateType.LeavingHome);
                    }
                    else if (ghosts[2].StateMachine.StateType == GhostStateType.Home)
                    {
                        ghosts[2].StateMachine.SetState(GhostStateType.LeavingHome);
                    }
                    else if (ghosts[1].StateMachine.StateType == GhostStateType.Home)
                    {
                        ghosts[1].StateMachine.SetState(GhostStateType.LeavingHome);
                    }

                    // Reset the timer
                    pelletTimer = pelletTimerStart;
                }

                // Reset the pellet timer if a pellet was eaten
                // Also check ghost release conditions
                if (pelletEaten)
                {
                    pelletTimer = pelletTimerStart;
                    pelletEaten = false;

                    // Release a ghost
                    if (Ghost.IsGlobalCounterActive)
                    {
                        Ghost.GlobalPelletCounter++;

                        // Check who's in home and release them if the counter == the limit.
                        // Original game behaviour means to only check if they're equal. Causes a reproducible house glitch.
                        if (ghosts[3].StateMachine.StateType == GhostStateType.Home && Ghost.GlobalPelletCounter == 7)
                        {
                            ghosts[3].StateMachine.SetState(GhostStateType.LeavingHome);
                        }
                        else if (ghosts[2].StateMachine.StateType == GhostStateType.Home && Ghost.GlobalPelletCounter == 17)
                        {
                            ghosts[2].StateMachine.SetState(GhostStateType.LeavingHome);
                        }
                        else if (ghosts[1].StateMachine.StateType == GhostStateType.Home && Ghost.GlobalPelletCounter == 32)
                        {
                            ghosts[1].StateMachine.SetState(GhostStateType.LeavingHome);
                        }
                    }
                    else
                    {
                        // Release a ghost if the pellet counter is >= the limit
                        // Otherwise increment the counter
                        if (ghosts[3].StateMachine.StateType == GhostStateType.Home)
                        {
                            if (ghosts[3].PelletCounter >= ghosts[3].PelletLimit)
                            {
                                ghosts[3].StateMachine.SetState(GhostStateType.LeavingHome);
                            }
                            else
                            {
                                ghosts[3].PelletCounter++;
                            }
                        }
                        else if (ghosts[2].StateMachine.StateType == GhostStateType.Home)
                        {
                            if (ghosts[2].PelletCounter >= ghosts[2].PelletLimit)
                            {
                                ghosts[2].StateMachine.SetState(GhostStateType.LeavingHome);
                            }
                            else
                            {
                                ghosts[2].PelletCounter++;
                            }
                        }
                        else if (ghosts[1].StateMachine.StateType == GhostStateType.Home)
                        {
                            if (ghosts[1].PelletCounter >= ghosts[1].PelletLimit)
                            {
                                ghosts[1].StateMachine.SetState(GhostStateType.LeavingHome);
                            }
                            else
                            {
                                ghosts[1].PelletCounter++;
                            }
                        }
                    }
                }

                #region Update - Ghosts

                // Update the ghosts
                for (int i = 0; i < 4; i++)
                {
                    ghosts[i].StateMachine.Update(halfDelta, args.Timing.UpdateCount, player.GridPosition, player.CurrentDirection,
                                                  ghosts[0].GridPosition);
                    ghosts[i].StateMachine.Update(halfDelta, args.Timing.UpdateCount, player.GridPosition, player.CurrentDirection,
                                                  ghosts[0].GridPosition, true);
                    HandleGhostCollision(ghosts[i]);
                }

                #endregion Update - Ghosts
            }

            #endregion State - Playing

            #region State - Player Death

            else if (currentState == GameState.PlayerDeath)
            {
                Ghost.IsGlobalCounterActive = true;

                // Set the volume of the intro player
                introPlayer.Volume = 0;

                // Set the frame if it isn't set
                if (levelStoppedFrame == -1)
                {
                    levelStoppedFrame = args.Timing.UpdateCount;
                }

                if (args.Timing.UpdateCount - levelStoppedFrame == 120)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        ghosts[i].Visible = false;
                    }

                    player.SetState(PlayerState.Dying);
                    player.AnimateDeath();
                }

                if (args.Timing.UpdateCount - levelStoppedFrame == 216)
                {
                    // Reduce the lives remaining
                    GameManager.LivesRemaining--;

                    // Set the level stopped frame
                    levelStoppedFrame = -1;

                    // Set the state to idle
                    currentState = GameState.Idle;

                    // If there are no lives remaining, quit to the main menu
                    if (GameManager.LivesRemaining < 0)
                    {
                        if (GameManager.IsNewHighScore) 
                        {
                            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                            {
                                await GameManager.SaveHighScore();

                                ContentDialog highScoreDialog = new ContentDialog
                                {
                                    Title = "New High Score",
                                    Content = $"You've achieved a new high score of {GameManager.HighScore:#,##0}",
                                    CloseButtonText = "OK"
                                };

                                _ = await highScoreDialog.ShowAsync();
                            });

                            Ghost.IsGlobalCounterActive = false;
                        }

                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            GameOver?.Invoke(this, EventArgs.Empty);
                        });
                    }
                    else 
                    {
                        // Reset everything
                        currentState = GameState.Intro;

                        foreach (Ghost ghost in ghosts)
                        {
                            ghost.Reset();
                        }

                        player.Reset();

                        canvasStatic.Invalidate();

                        introPlayer.Play();
                    }
                }
                else if (args.Timing.UpdateCount - levelStoppedFrame >= 120 && (args.Timing.UpdateCount - levelStoppedFrame) % 8 == 0)
                {
                    player.AnimateDeath();
                }
            }

            #endregion State - Player Death

            #region State - Level Complete

            else if (currentState == GameState.Complete) 
            {
                Ghost.IsGlobalCounterActive = false;

                // Set the frame if it isn't set
                if (levelStoppedFrame == -1)
                {
                    levelStoppedFrame = args.Timing.UpdateCount;
                }

                // Wait for 120 frames (~2 seconds) before starting the maze blinking
                if (args.Timing.UpdateCount - levelStoppedFrame == 120)
                {
                    maze.Sprite = endSprites[0];
                    foreach (Ghost ghost in ghosts)
                    {
                        ghost.Visible = false;
                    }
                    canvasStatic.Invalidate();
                }

                // Change the sprite every ~2/3 second after the initial 2 seconds, ending with blue
                else if (args.Timing.UpdateCount - levelStoppedFrame == 160)
                {
                    maze.Sprite = endSprites[1];
                    canvasStatic.Invalidate();
                }

                else if (args.Timing.UpdateCount - levelStoppedFrame == 200)
                {
                    maze.Sprite = endSprites[0];
                    canvasStatic.Invalidate();
                }

                else if (args.Timing.UpdateCount - levelStoppedFrame == 240)
                {
                    maze.Sprite = endSprites[1];
                    canvasStatic.Invalidate();
                }

                else if (args.Timing.UpdateCount - levelStoppedFrame == 280)
                {
                    maze.Sprite = endSprites[0];
                    canvasStatic.Invalidate();
                }

                else if (args.Timing.UpdateCount - levelStoppedFrame == 320)
                {
                    maze.Sprite = endSprites[1];
                    canvasStatic.Invalidate();
                }

                else if (args.Timing.UpdateCount - levelStoppedFrame == 360)
                {
                    maze.Sprite = endSprites[0];
                    canvasStatic.Invalidate();
                }

                else if (args.Timing.UpdateCount - levelStoppedFrame == 400)
                {
                    maze.Sprite = endSprites[1];
                    canvasStatic.Invalidate();
                }

                else if (args.Timing.UpdateCount - levelStoppedFrame == 440)
                {
                    // Invoke the level complete event
                    // Need to use Dispatcher to run on the UI thread as this loop is in the game thread
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => LevelComplete?.Invoke(this, EventArgs.Empty));
                }
            }

            #endregion State - Level Complete

            #region Blinking Text and Pellets

            // If not in the intro, blink the power pellets every 15 frames.
            if (currentState != GameState.Intro)
            {
                if (powerPellets.Count > 0 && args.Timing.UpdateCount % 15 == 0)
                {
                    if (powerPellets.Count > 0)
                    {
                        for (int i = 0; i < powerPellets.Count; i++)
                        {
                            powerPellets[i].Visible = !powerPellets[i].Visible;
                        }
                    }
                }
            }

            // Swap the 1UP text every 30 frames
            if (args.Timing.UpdateCount % 30 == 0)
            {
                oneUpVisible = !oneUpVisible;
            }

            #endregion Blinking Text and Pellets
        }

        #endregion Update Methods



        //  Needed to avoid memory leaks for the canvas objects
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            canvasAnimated.RemoveFromVisualTree();
            canvasStatic.RemoveFromVisualTree();

            canvasAnimated = null;
            canvasStatic = null;

            introPlayer.Dispose();
            fruitPlayer.Dispose();

            introPlayer = null;
            fruitPlayer = null;

            // Unregister the input event handlers
            Window.Current.CoreWindow.KeyDown -= coreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp -= coreWindow_KeyUp;
        }
    }
}
