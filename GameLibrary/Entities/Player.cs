using Microsoft.Graphics.Canvas;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace GameLibrary
{
    // Player doesn't have many states so we'll use a simple enum
    /// <summary>
    /// An enum representing the state of the player.
    /// </summary>
    public enum PlayerState
    {
        Dying,
        Idle,
        LevelComplete,
        Moving
    }

    /// <summary>
    /// Class representing the player in the game.
    /// </summary>
    public sealed class Player : MovingEntity, IResourceLoader
    {
        #region Player Frames

        /// <summary>
        /// An array of CanvasBitmaps that represent the player frames moving left.
        /// </summary>
        private CanvasBitmap[] leftFrames = new CanvasBitmap[4];

        /// <summary>
        /// An array of CanvasBitmaps that represent the player frames moving right.
        /// </summary>
        private CanvasBitmap[] rightFrames = new CanvasBitmap[4];

        /// <summary>
        /// An array of CanvasBitmaps that represent the player frames moving up.
        /// </summary>
        private CanvasBitmap[] upFrames = new CanvasBitmap[4];

        /// <summary>
        /// An array of CanvasBitmaps that represent the player frames moving left.
        /// </summary>
        private CanvasBitmap[] downFrames = new CanvasBitmap[4];

        /// <summary>
        /// An array of CanvasBitmaps that represent the player frames when dying.
        /// </summary>
        private CanvasBitmap[] deathAnimation = new CanvasBitmap[12];

        #endregion Player Frames

        #region Fields

        /// <summary>
        /// The current state of the player.
        /// </summary>
        private PlayerState currentState = PlayerState.Idle;

        /// <summary>
        /// Speed data for the player.
        /// </summary>
        private (uint, uint) speedData;

        /// <summary>
        /// Whether or not the player will move this update.
        /// </summary>
        private bool willMove = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The current state of the player.
        /// </summary>
        public PlayerState CurrentState => currentState;

        /// <summary>
        /// Has the player eaten a power pellet.
        /// </summary>
        public bool IsEnergized { get; private set; } = false;

        #endregion Properties

        #region Properties - Static

        /// <summary>
        /// How many ghosts have been eaten in the current energized state.
        /// </summary>
        public static int GhostMultiplier { get; set; } = 1;

        #endregion Properties - Static

        #region Constructors

        public Player(double centerX, double centerY, Size size) : base(centerX, centerY, size)
        {
            // Set the starting direction
            CurrentDirection = Direction.Left;
            DesiredDirection = Direction.Left;

            // Get the speed data
            speedData = PlayerSpeedData.GetSpeedData();

            // Get the frightened data for the current level
            FrightenedData = GhostFrightenedData.GetFrightenedData();
        }

        #endregion Constructors

        #region Methods - Private

        private void Move() 
        {
            // Check if the player can move
            if (Map.GetNextTile(CurrentDirection, gridPosition) == TileType.Wall && distanceToCenter.Equals(PointExtensions.ZERO))
            {
                SetState(PlayerState.Idle);

                // Set the frame to an open mouth
                CurrentFrame = 0;
                SetSpriteDirection();

                return;
            }

            if (currentState == PlayerState.Moving)
            {
                // Check whether player is energized before checking if they'll move
                // If the 16th bit of the speed is set, the player will move
                if (IsEnergized)
                {
                    willMove = speedData.Item2.GetBit(16) == 1;

                    // Rotate the bits
                    speedData.Item2 = speedData.Item2.BitRotateRight(1);
                }
                else 
                {
                    willMove = speedData.Item1.GetBit(16) == 1;

                    // Rotate the bits
                    speedData.Item1 = speedData.Item1.BitRotateRight(1);
                }

                // Don't proceed if no movement will occur
                if (!willMove)
                {
                    return;
                }

                // Set based on current direction
                switch (CurrentDirection)
                {
                    case Direction.Up:
                        CenterY -= 1;
                        break;
                    case Direction.Down:
                        CenterY += 1;
                        break;
                    case Direction.Left:
                        CenterX -= 1;
                        break;
                    case Direction.Right:
                        CenterX += 1;
                        break;
                }
            }

            // Handle wrapping on the page
            if (Map.GetNextTile(CurrentDirection, gridPosition) == TileType.Teleport)
            {
                switch (CurrentDirection)
                {
                    case Direction.Left:
                        CenterX = 30 * Constants.TILE_SIZE;
                        break;
                    case Direction.Right:
                        CenterX = -2 * Constants.TILE_SIZE;
                        break;
                }
            }
        }

        #endregion Methods - Private

        #region Methods - Public

        /// <summary>
        /// Animate the player's death.
        /// </summary>
        public void AnimateDeath()
        {
            // If the current frame is out of bounds, reset it
            if (CurrentFrame == 12)
            {
                currentState = PlayerState.Idle;
                return;
            }

            // Set the sprite based on the direction
            Sprite = deathAnimation[CurrentFrame];

            // Increment the frame
            CurrentFrame++;
        }

        /// <summary>
        /// Call when the player eats a power pellet.
        /// </summary>
        public void Energize()
        {
            // If the level is 17 and up (excluding 18) , the player will not be energized
            if (GameManager.CurrentLevel >= 17 && GameManager.CurrentLevel != 18)
            {
                return;
            }

            // Reset the multiplier
            GhostMultiplier = 1;

            // Set the frightened timer which is also how long the player is energized
            FrightenedTimer = FrightenedData.Item1;
            IsEnergized = true;
        }

        /// <summary>
        /// Sets the state of the player.
        /// </summary>
        /// <param name="state">The new state of the player</param>
        public void SetState(PlayerState state)
        {
            // Handle the level complete state
            if (state == PlayerState.LevelComplete)
            {
                // Set the frame to the shared frame
                CurrentFrame = 2;
                SetSpriteDirection();

                // Set player back to idle
                currentState = PlayerState.Idle;
            }

            if (state == PlayerState.Dying)
            {
                // Set the frame to the shared frame
                CurrentFrame = 0;

                // Play the death audio
                MediaPlayer deathPlayer = new MediaPlayer() 
                {
                    Volume = 0.05,
                    Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Sounds/pacman_death.wav"))
                };

                deathPlayer.MediaEnded += (sender, e) =>
                {
                    deathPlayer.Dispose();
                    deathPlayer = null;
                };

                deathPlayer.Play();
            }

            currentState = state;
        }

        /// <summary>
        /// The players update loop.
        /// </summary>
        /// <param name="delta">Time the last frame took to complete in seconds.</param>
        /// <param name="updateCount">The number of updates that have occurred.</param>
        /// <param name="isSecondUpdate">Is this the second update of the frame?</param>
        public void Update(double delta, long updateCount, bool isSecondUpdate = false)
        {
            // Skip frames if the player is frozen
            if (FreezeFramesRemaining > 0)
            {
                FreezeFramesRemaining--;
                return;
            }

            // Decrease the frightened timer if the player is energized
            if (isSecondUpdate && IsEnergized && FrightenedTimer > 0)
            {
                FrightenedTimer--;
            }
            else if (isSecondUpdate && IsEnergized && FrightenedTimer == 0)
            {
                // Reset the ghost multiplier
                GhostMultiplier = 1;

                // Reset energized
                IsEnergized = false;
            }

            // Always update direction
            // If the directions are different, change the direction if applicable
            if (DesiredDirection != CurrentDirection && DesiredDirection != Direction.None)
            {
                // Only set the direction if the player wouldn't be blocked
                TileType nextTile = Map.GetTileAt(DesiredDirection.ToPoint().Add(gridPosition));

                if (nextTile != TileType.Wall && nextTile != TileType.HomeDoor)
                {
                    CurrentDirection = DesiredDirection;
                    DesiredDirection = Direction.None;

                    SetState(PlayerState.Moving);
                }
            }

            // If the player is idle, don't update
            if (currentState == PlayerState.Idle)
            {
                return;
            }

            // Move the player
            Move();

            // Animate the player body every 2 frames
            if (isSecondUpdate && updateCount % 2 == 0)
            {
                AnimateBody();
            }

            // Snap to the center of the tile for cornering (can turn up to 4 pixels earlier than center)
            // Add one pixel at a time each Update until the player is at the center, for smoothing.
            if (CurrentDirection.IsHorizontal() && DistanceToCenter.Y != 0)
            {
                CenterY += DirectionToCenter.Y;
            }
            else if (!CurrentDirection.IsHorizontal() && DistanceToCenter.X != 0)
            {
                CenterX += DirectionToCenter.X;
            }
        }

        #endregion Methods - Public

        #region Methods - Implemented

        public async Task LoadResources(ICanvasResourceCreator sender)
        {
            // Load the shared sprite first
            CanvasBitmap sharedFrame = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/shared.png"));

            // Load the left frames
            leftFrames[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/left_2.png"));
            leftFrames[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/left_1.png"));
            leftFrames[2] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/left_2.png"));
            leftFrames[3] = sharedFrame;

            // Load the right frames
            rightFrames[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/right_2.png"));
            rightFrames[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/right_1.png"));
            rightFrames[2] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/right_2.png"));
            rightFrames[3] = sharedFrame;

            // Load the up frames
            upFrames[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/up_2.png"));
            upFrames[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/up_1.png"));
            upFrames[2] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/up_2.png"));
            upFrames[3] = sharedFrame;

            // Load the down frames
            downFrames[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/down_2.png"));
            downFrames[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/down_1.png"));
            downFrames[2] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/down_2.png"));
            downFrames[3] = sharedFrame;

            // Load the death animation
            for (int i = 0; i < 11; i++) 
            {
                deathAnimation[i] = await CanvasBitmap.LoadAsync(sender, new Uri($"ms-appx:///Assets/Pacman/Death/death_{i + 1}.png"));
            }

            deathAnimation[11] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Pacman/Death/death_empty.png"));

            // Set the sprite to the shared frame
            Sprite = sharedFrame;
        }

        #endregion Methods - Implemented

        #region Methods - Overriden

        public override void AnimateBody()
        {
            // If the current frame is out of bounds, reset it
            if (CurrentFrame == 4)
            {
                CurrentFrame = 0;
            }

            // Set the sprite based on the direction
            SetSpriteDirection();

            // Increment the frame
            CurrentFrame++;
        }

        public override void Draw(CanvasDrawingSession drawingSession)
        {
            // Draw the object
            if (currentState == PlayerState.Dying)
            {
                drawingSession.DrawImage(
                    Sprite,
                    new Rect(
                        Math.Floor(CenterX + DrawOffset.X),
                        Math.Floor(CenterY + DrawOffset.Y),
                        15,
                        13
                    )
                );
            }
            else 
            {
                base.Draw(drawingSession);
            }
        }

        public override void Reset()
        {
            // Reset the position
            CenterX = InitialPosition.X;
            CenterY = InitialPosition.Y;

            // Reset the multiplier
            GhostMultiplier = 1;

            // Reset the energized state
            IsEnergized = false;

            // Reset the direction
            CurrentDirection = Direction.Left;
            DesiredDirection = Direction.Left;

            // Reset the freeze frames
            FreezeFramesRemaining = 0;

            // Get fresh speed data
            speedData = PlayerSpeedData.GetSpeedData();

            // Reset the state
            SetState(PlayerState.Idle);

            CurrentFrame = 3;
            SetSpriteDirection();
        }

        public override void SetSpriteDirection()
        {
            // Set the sprite based on the direction
            switch (CurrentDirection)
            {
                case Direction.Left:
                    Sprite = leftFrames[CurrentFrame];
                    break;
                case Direction.Right:
                    Sprite = rightFrames[CurrentFrame];
                    break;
                case Direction.Up:
                    Sprite = upFrames[CurrentFrame];
                    break;
                case Direction.Down:
                    Sprite = downFrames[CurrentFrame];
                    break;
            }
        }

        #endregion Methods - Overriden
    }
}
