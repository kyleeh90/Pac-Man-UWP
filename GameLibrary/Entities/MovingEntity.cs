using Microsoft.Graphics.Canvas;
using System;
using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// Base class for all moving entities in the game.
    /// </summary>
    public abstract class MovingEntity
    {
        #region Fields

        protected double centerX, centerY;
        protected Point distanceToCenter, gridPosition, positionInTile, previousGridPosition;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The horizontal center of the object, in pixels.
        /// </summary>
        public virtual double CenterX
        {
            get => centerX;
            set
            {
                centerX = value;

                previousGridPosition.X = gridPosition.X;

                gridPosition.X = Math.Floor(centerX / Constants.TILE_SIZE);

                positionInTile.X = Math.Floor(centerX % Constants.TILE_SIZE);

                distanceToCenter.X = Constants.TILE_CENTER.X - positionInTile.X;
            }
        }

        /// <summary>
        /// The vertical center of the object, in pixels.
        /// </summary>
        public virtual double CenterY
        {
            get => centerY;
            set
            {
                centerY = value;

                previousGridPosition.Y = gridPosition.Y;

                gridPosition.Y = Math.Floor(centerY / Constants.TILE_SIZE);

                positionInTile.Y = Math.Floor(centerY % Constants.TILE_SIZE);

                distanceToCenter.Y = Constants.TILE_CENTER.Y - positionInTile.Y;
            }
        }

        /// <summary>
        /// The current direction of the object.
        /// </summary>
        public Direction CurrentDirection { get; set; }

        /// <summary>
        /// Current frame of the animation.
        /// </summary>
        protected int CurrentFrame { get; set; } = 0;

        /// <summary>
        /// The desired direction of the object.
        /// </summary>
        public Direction DesiredDirection { get; set; }

        /// <summary>
        /// Get a directional point of which way the center of the tile is.
        /// </summary>
        protected Point DirectionToCenter => new Point(Math.Sign(distanceToCenter.X), Math.Sign(distanceToCenter.Y));

        /// <summary>
        /// The distance to the center of the object's current tile.
        /// </summary>
        protected Point DistanceToCenter => new Point(Math.Abs(distanceToCenter.X), Math.Abs(distanceToCenter.Y));

        /// <summary>
        /// How many pixels is drawing offset by from the center of the entity in pixels.
        /// </summary>
        protected Point DrawOffset { get; set; } = new Point(-6, -6);

        /// <summary>
        /// How many frames does the entity have to freeze for.
        /// </summary>
        public int FreezeFramesRemaining { get; set; } = 0;

        /// <summary>
        /// The frightened data containing the time to be frightened, and when the ghosts start blinking.
        /// </summary>
        public static (int, int) FrightenedData { get; protected set; }

        /// <summary>
        /// Frightened timer is shared between the player and the ghosts.
        /// </summary>
        public static int FrightenedTimer { get; set; } = 0;

        /// <summary>
        /// The current grid position of the object.
        /// </summary>
        public Point GridPosition => gridPosition;

        /// <summary>
        /// The initial position of the object.
        /// </summary>
        public Point InitialPosition { get; protected set; }

        /// <summary>
        /// The size of the object, in pixels.
        /// </summary>
        protected Size Size { get; set; }

        /// <summary>
        /// The currently displayed sprite.
        /// </summary>
        public CanvasBitmap Sprite { get; protected set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructs a new MovingEntity object.
        /// </summary>
        /// <param name="centerX">The center of the object, in pixels.</param>
        /// <param name="centerY">The center of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        protected MovingEntity(double centerX, double centerY, Size size)
        {
            // Set the positions
            CenterX = centerX;
            CenterY = centerY;

            // Set the size
            Size = size;

            // Set the initial position
            InitialPosition = new Point(centerX, centerY);
        }

        #endregion Constructors

        #region Methods - Abstract

        /// <summary>
        /// Animate the body of the entity.
        /// </summary>
        public abstract void AnimateBody();

        /// <summary>
        /// Move the entity by the specified amount.
        /// </summary>
        /// <param name="moveAmount">Amount to move by, in pixels.</param>
        public virtual void Move(double moveAmount) { }

        /// <summary>
        /// Reset the entity to its starting state.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Sets the sprite based on the current direction.
        /// </summary>
        public abstract void SetSpriteDirection();

        #endregion Methods - Abstract

        #region Methods - Virtual

        /// <summary>
        /// Draws the object to the screen.
        /// </summary>
        /// <param name="drawingSession">The CanvasDrawingSession used for drawing.</param>
        public virtual void Draw(CanvasDrawingSession drawingSession)
        {
            // Draw the object
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

        #endregion Methods - Virtual
    }
}
