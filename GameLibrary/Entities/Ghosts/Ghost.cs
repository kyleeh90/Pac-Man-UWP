using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// Enum for the different types of ghosts.
    /// </summary>
    public enum GhostType
    {
        Blinky,
        Clyde,
        Inky,
        Pinky
    }

    /// <summary>
    /// Base class for all ghosts in the game.
    /// </summary>
    public abstract class Ghost : MovingEntity, ICollectible
    {
        #region Fields - Static

        /// <summary>
        /// List of every direction a ghost can go in.
        /// </summary>
        protected readonly static List<Direction> startingDirections = new List<Direction>
        {
            Direction.Up,
            Direction.Left,
            Direction.Down,
            Direction.Right
        };

        #endregion Fields - Static

        #region Fields

        /// <summary>
        /// Private backing field for IsFrightened.
        /// </summary>
        private bool isFrightened = false;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The frames representing the ghost's animation.
        /// </summary>
        protected CanvasBitmap[] AnimationFrames { get; set; }

        public override double CenterX 
        { 
            get => centerX;
            set 
            {
                base.CenterX = value;

                positionInTile.X = Math.Floor(centerX % Constants.TILE_SIZE);

                distanceToCenter.X = Constants.TILE_CENTER.X - (positionInTile.X - 3);
            }
        }

        public override double CenterY 
        { 
            get => centerY;
            set 
            {
                base.CenterY = value;

                positionInTile.Y = Math.Floor(centerY % Constants.TILE_SIZE);

                distanceToCenter.Y = Constants.TILE_CENTER.Y - (positionInTile.Y - 3);
            }
        }

        /// <summary>
        /// The current state of the ghost.
        /// </summary>
        public GhostStateType CurrentState => StateMachine.StateType;

        /// <summary>
        /// The current tile type the ghost is on.
        /// </summary>
        public TileType CurrentTileType { get; set; }

        /// <summary>
        /// The current eyes sprite of the ghost.
        /// </summary>
        protected CanvasBitmap CurrentEyes { get; set; }

        /// <summary>
        /// Whether to draw the frightened white or blue sprites.
        /// </summary>
        public bool DrawFrightenedWhite { get; set; } = false;

        /// <summary>
        /// If true, the ghost will reverse direction at the next tile.
        /// </summary>
        public bool ForceReverse { get; set; } = false;

        /// <summary>
        /// The type of ghost.
        /// </summary>
        public GhostType GhostType { get; private set; }

        /// <summary>
        /// Is the ghost frightened?
        /// </summary>
        public bool IsFrightened 
        {
            get => isFrightened;
            set 
            {
                // Can't set frightened if going home or entering home
                if (CurrentState == GhostStateType.GoingHome || CurrentState == GhostStateType.EnteringHome)
                {
                    return;
                }
                
                // Only reverse the ghost if it's going into frightened
                if (value == true)
                {
                    DrawFrightenedWhite = false;
                    previousGridPosition = gridPosition;

                    // If in the house leave right to true
                    if (CurrentState == GhostStateType.Home)
                    {
                        LeaveRight = true;
                    }
                    // Otherwise set force reverse
                    else
                    {
                        ForceReverse = true;
                    }
                }

                // If the level is 17 and up (excluding 18) , the ghost will not be frightened
                if (GameManager.CurrentLevel >= 17 && GameManager.CurrentLevel != 18)
                {
                    isFrightened = false;
                    return;
                }

                isFrightened = value;
            }
        }

        /// <summary>
        /// Whether the ghost leaves home to the right.
        /// </summary>
        public bool LeaveRight { get; set; } = false;

        /// <summary>
        /// The tile type ahead of the ghost.
        /// </summary>
        public TileType NextTileType { get; set; }

        /// <summary>
        /// Personal pellet counter for when to leave home.
        /// </summary>
        public int PelletCounter { get; set; } = 0;

        /// <summary>
        /// When the pellet counter reaches this limit, the ghost will leave home.
        /// </summary>
        public int PelletLimit { get; protected set; } = 0;

        public int Points => 200;

        /// <summary>
        /// The previous grid position of the ghost.
        /// </summary>
        public Point PreviousGridPosition 
        {
            get => previousGridPosition;
            set => previousGridPosition = value;
        }

        /// <summary>
        /// The target tile for the ghost to move to in scatter mode, in grid units.
        /// </summary>
        public Point ScatterTarget { get; protected set; }

        /// <summary>
        /// The speed of the ghost. Either 1 or 0.
        /// </summary>
        public uint Speed { get; set; } = 0;

        /// <summary>
        /// An array containing the speed of the ghosts for the current level.
        /// </summary>
        /// <remarks>In order: Base speed, tunnel speed, frightened speed.</remarks>
        public uint[] SpeedData { get; private set; }

        /// <summary>
        /// The state machine for the ghost.
        /// </summary>
        public GhostStateMachine StateMachine { get; private set; }

        /// <summary>
        /// The target tile for the ghost to move to, in grid units.
        /// </summary>
        public Point TargetTile { get; set; }

        /// <summary>
        /// Whether the ghost is visible.
        /// </summary>
        public bool Visible { get; set; } = true;

        #endregion Properties

        #region Properties - Static

        /// <summary>
        /// The shared eye sprites for all ghosts.
        /// </summary>
        protected static CanvasBitmap[] Eyes { get; set; }

        /// <summary>
        /// Shared blue frightened sprites for the ghost.
        /// </summary>
        protected static CanvasBitmap[] FrightenedFramesBlue { get; set; }

        /// <summary>
        /// Shared white frightened sprites for the ghost.
        /// </summary>
        protected static CanvasBitmap[] FrightenedFramesWhite { get; set; }

        /// <summary>
        /// The global pellet counter for when to leave home.
        /// </summary>
        public static int GlobalPelletCounter { get; set; } = 0;

        /// <summary>
        /// Whether to use the global counter for when to leave home or personal pellet counters.
        /// </summary>
        public static bool IsGlobalCounterActive { get; set; } = false;

        /// <summary>
        /// The time to stay in each state for this level, in frames.
        /// </summary>
        public static int[] StateTimes { get; private set; }

        #endregion Properties - Static

        #region Constructors

        /// <summary>
        /// Constructs a new Ghost object.
        /// </summary>
        /// <param name="centerX">The center of the object, in pixels.</param>
        /// <param name="centerY">The center of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        /// <param name="ghostType">The type of ghost.</param>
        protected Ghost(double centerX, double centerY, Size size, GhostType ghostType) : base(centerX, centerY, size) 
        {
            // Get the speed data
            SpeedData = GhostSpeedData.GetSpeedData();

            // Set the draw offset
            DrawOffset = new Point(-9, -9);

            // Set the ghost type
            GhostType = ghostType;

            // Initialize the state machine
            StateMachine = new GhostStateMachine(this);

            // Set the initial position
            previousGridPosition = gridPosition;
        }

        #endregion Constructors

        #region Methods - Static

        /// <summary>
        /// Initializes the static properties for the Ghost class.
        /// </summary>
        public static void InitializeData() 
        {
            // Get the state time data
            StateTimes = GhostStateTimes.GetStateTimes();
        }

        /// <summary>
        /// Loads the resources needed for drawing.
        /// </summary>
        /// <param name="sender">The CanvasControl or AnimatedCanvasControl that is calling this method</param>
        public static async Task LoadSharedResources(ICanvasResourceCreator sender)
        {
            // Load the eyes
            Eyes = new CanvasBitmap[4];

            Eyes[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/eyes_up.png"));
            Eyes[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/eyes_left.png"));
            Eyes[2] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/eyes_down.png"));
            Eyes[3] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/eyes_right.png"));

            // Load the frightened sprites
            FrightenedFramesBlue = new CanvasBitmap[2];
            FrightenedFramesBlue[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/frightened_blue_1.png"));
            FrightenedFramesBlue[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/frightened_blue_2.png"));

            FrightenedFramesWhite = new CanvasBitmap[2];
            FrightenedFramesWhite[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/frightened_white_1.png"));
            FrightenedFramesWhite[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/frightened_white_2.png"));
        }

        #endregion Methods - Static

        #region Methods - Public

        /// <summary>
        /// Get the best direction to move in.
        /// </summary>
        /// <returns>Returns the best direction to turn at the next turn.</returns>
        public Direction GetBestDirection()
        {
            // Create a list to store the valid directions
            List<Direction> validDirections = new List<Direction>();

            // Get the reverse of the current direction
            Direction reversedDirection = CurrentDirection.Reversed();

            // Loop through all the directions
            for (int i = 0; i < 4; i++)
            {
                // Skip the reversed direction
                if (startingDirections[i] == reversedDirection)
                {
                    continue;
                }

                // If the current tile is restricted, skip up
                if (NextTileType == TileType.Restricted && startingDirections[i] == Direction.Up)
                {
                    continue;
                }

                // Calculate the new grid position to measure against
                Point newPosition = gridPosition.Add(startingDirections[i].ToPoint()).Add(CurrentDirection.ToPoint());

                // Get the next tile
                TileType nextTile = Map.GetTileAt(newPosition);

                if (nextTile != TileType.Wall && nextTile != TileType.HomeDoor)
                {
                    validDirections.Add(startingDirections[i]);
                }
            }

            // If there's only one valid direction, set it
            if (validDirections.Count == 1)
            {
                return validDirections[0];
            }

            // Otherwise, loop through again and calculate the distance to the target tile
            // Also deal with tie breakers where distance is equal
            Direction bestDirection = Direction.None;

            // Get a distance to compare against
            double minDistance = 10000;

            // Loop through all the valid directions
            for (int i = 0; i < validDirections.Count; i++)
            {
                // Calculate the new grid position to measure against
                Point newPosition = gridPosition.Add(validDirections[i].ToPoint().Add(CurrentDirection.ToPoint()));

                // Get the distance to the target tile
                double distance = Math.Sqrt((TargetTile.X - newPosition.X).Squared() + (TargetTile.Y - newPosition.Y).Squared());

                // If the distance is less than the minimum distance, set the new minimum distance
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestDirection = validDirections[i];
                }
                // Otherwise compare enum values, less is better
                else if (distance == minDistance && validDirections[i] < bestDirection)
                {
                    bestDirection = validDirections[i];
                }
            }

            // Set the desired direction
            return bestDirection;
        }

        /// <summary>
        /// Gets a random direction for the ghost to move in.
        /// </summary>
        /// <returns>A random valid Direction.</returns>
        public Direction GetRandomDirection()
        {
            // First get a random direction
            Direction newDirection = RngSimulator.GetDirection();

            // If the new directions is valid, set it. Otherwise pick a direction clockwise until valid
            if (IsValidMove(newDirection) && newDirection != CurrentDirection.Reversed())
            {
                return newDirection;
            }
            else
            {
                newDirection = newDirection.RotatedClockwise();

                while (!IsValidMove(newDirection) || newDirection == CurrentDirection.Reversed())
                {
                    newDirection = newDirection.RotatedClockwise();
                }

                return newDirection;
            }
        }

        /// <summary>
        /// Move the ghost towards a target X position in pixels.
        /// </summary>
        /// <param name="moveAmount">The amount to move the object by, in pixels per second.</param>
        /// <param name="X">The target X to move towards.</param>
        public void MoveTowardsX(double moveAmount, double X)
        {
            // Get the direction of the target
            int direction = Math.Sign(X - CenterX);

            // Move the ghost
            CenterX += moveAmount * direction;
        }

        /// <summary>
        /// Move the the ghost towards a target Y position in pixels.
        /// </summary>
        /// <param name="moveAmount">The amount to move the object by, in pixels per second.</param>
        /// <param name="Y">The target Y to move towards.</param>
        public void MoveTowardsY(double moveAmount, double Y)
        {
            // Get the direction of the target
            int direction = Math.Sign(Y - CenterY);

            // Move the ghost
            CenterY += moveAmount * direction;
        }

        #endregion Methods - Public

        #region Methods - Protected

        /// <summary>
        /// Checks if a move is valid in a given direction.
        /// </summary>
        /// <param name="toCheck">The direction to check</param>
        /// <returns>True if the given move is valid, false otherwise.</returns>
        protected bool IsValidMove(Direction toCheck)
        {
            TileType checkedTile = Map.GetTileAt(GridPosition.Add(toCheck.ToPoint()));

            if (checkedTile != TileType.Wall && checkedTile != TileType.HomeDoor)
            {
                return true;
            }

            return false;
        }

        #endregion Methods - Protected

        #region Methods - Virtual

        /// <summary>
        /// Set the target tile for the ghost.
        /// </summary>
        /// <param name="playerPosition">The position of the player, in grid units.</param>
        public virtual void SetChaseTarget(Point playerPosition)
        {
            // Set the target tile to the player's position
            TargetTile = playerPosition;
        }

        /// <summary>
        /// Set the target tile for the ghost.
        /// </summary>
        /// <param name="playerPosition">The position of the player, in grid units.</param>
        /// <param name="playerFacing">The direction the player is facing.</param>
        public virtual void SetChaseTarget(Point playerPosition, Direction playerFacing) { }

        /// <summary>
        /// Set the target tile for the ghost.
        /// </summary>
        /// <param name="playerPosition">The position of the player, in grid units.</param>
        /// <param name="playerFacing">The direction the player is facing.</param>
        /// <param name="blinkyPosition">The position of Blinky, in grid units.</param>
        public virtual void SetChaseTarget(Point playerPosition,Direction playerFacing, Point blinkyPosition) { }

        #endregion Methods - Virtual

        #region Methods - Overridden

        public override void AnimateBody()
        {
            // Set the sprite based on the direction
            SetSpriteDirection();

            // Increment the frame
            CurrentFrame++;

            // If the current frame is out of bounds, reset it
            if (CurrentFrame == 2)
            {
                CurrentFrame = 0;
            }
        }

        public override void Draw(CanvasDrawingSession drawingSession)
        {
            // If the ghost is dead, draw the score and return
            if (CurrentState == GhostStateType.Dead)
            {
                drawingSession.DrawImage(
                    GhostDead.ScoreSprite,
                    new Rect(
                        Math.Floor(CenterX + DrawOffset.X),
                        Math.Floor(CenterY + DrawOffset.Y),
                        16,
                        7
                    )
                );

                return;
            }

            // Set the sprite based on the mode
            if (IsFrightened && DrawFrightenedWhite)
            {
                Sprite = FrightenedFramesWhite[CurrentFrame];
            }
            else if (IsFrightened)
            {
                Sprite = FrightenedFramesBlue[CurrentFrame];
            }
            else 
            {
                Sprite = AnimationFrames[CurrentFrame];
            }

            // Draw the ghost if not going home, then only eyes are drawn.
            if (CurrentState != GhostStateType.GoingHome && CurrentState != GhostStateType.EnteringHome)
            {
                drawingSession.DrawImage(
                    Sprite,
                    new Rect(
                        Math.Floor(CenterX + DrawOffset.X),
                        Math.Floor(CenterY + DrawOffset.Y),
                        Size.Width,
                        Size.Height
                    )
                );
            }

            // Only draw the eyes if not frightened
            if (!IsFrightened)
            {
                drawingSession.DrawImage(
                    CurrentEyes,
                    new Rect(
                        Math.Floor(CenterX + DrawOffset.X),
                        Math.Floor(CenterY + DrawOffset.Y),
                        Size.Width,
                        Size.Height
                    )
                );
            }
        }

        public override void Move(double moveAmount)
        {
            // Turn at intersections and restricted intersection when you're at the center
            // Show the direction of the next turn when they enter the intersection
            if (CurrentTileType == TileType.Intersection || CurrentTileType == TileType.Restricted)
            {
                SetSpriteDirection();
                if (distanceToCenter.Equals(PointExtensions.ZERO))
                {
                    // If the ghost is at the center of the intersection, set the direction
                    CurrentDirection = DesiredDirection;
                }
            }

            // Set based on current direction
            switch (CurrentDirection)
            {
                case Direction.Up:
                    CenterY -= moveAmount;
                    break;
                case Direction.Down:
                    CenterY += moveAmount;
                    break;
                case Direction.Left:
                    CenterX -= moveAmount;
                    break;
                case Direction.Right:
                    CenterX += moveAmount;
                    break;
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

        public override void Reset()
        {
            // Reset the position
            CenterX = InitialPosition.X;
            CenterY = InitialPosition.Y;

            // Set visibility
            Visible = true;

            // Get fresh speed data
            SpeedData = GhostSpeedData.GetSpeedData();

            // Reset the state machine
            StateMachine.SetState(GhostStateType.Idle);

            CurrentFrame = 0;
        }

        public override void SetSpriteDirection() => CurrentEyes = Eyes[(int)DesiredDirection];

        #endregion Methods - Overridden
    }
}