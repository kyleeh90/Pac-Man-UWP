using Microsoft.Graphics.Canvas;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// Class for the Clyde ghost.
    /// </summary>
    public sealed class Clyde : Ghost, IResourceLoader
    {
        #region Constructors

        /// <summary>
        /// Constructs a new Clyde object.
        /// </summary>
        /// <param name="centerX">The center of the object, in pixels.</param>
        /// <param name="centerY">The center of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        public Clyde(double centerX, double centerY, Size size) : base(centerX, centerY, size, GhostType.Clyde) 
        {
            // Set the scatter target
            ScatterTarget = new Point(0, 34);

            // Clyde has two pellet limits, and then 0 for the remainder of the game
            if (GameManager.CurrentLevel == 1)
            {
                PelletLimit = 60;
            }
            else if (GameManager.CurrentLevel == 2)
            {
                PelletLimit = 50;
            }
        }

        #endregion Constructors

        #region Methods - Implemented

        public async Task LoadResources(ICanvasResourceCreator sender)
        {
            AnimationFrames = new CanvasBitmap[2];

            AnimationFrames[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Clyde/body_1.png"));
            AnimationFrames[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Clyde/body_2.png"));

            // Set the sprite
            Sprite = AnimationFrames[0];

            // Set the direction and eyes
            CurrentDirection = Direction.Up;
            DesiredDirection = Direction.Up;
            SetSpriteDirection();
        }

        #endregion Methods - Implemented

        #region Methods - Overriden

        public override void Reset()
        {
            base.Reset();

            CurrentDirection = Direction.Up;
            DesiredDirection = Direction.Up;

            SetSpriteDirection();
        }

        public override void SetChaseTarget(Point playerPosition)
        {
            // Get distance to player. Clyde targets the player directly if the distance is greater than or equal to 8 tiles.
            // Otherwise they target the scatter target.
            double distance = Math.Sqrt((playerPosition.X - GridPosition.X).Squared() + (playerPosition.Y - GridPosition.Y).Squared());

            if (distance >= 8)
            {
                TargetTile = playerPosition;
            }
            else
            {
                TargetTile = ScatterTarget;
            }
        }

        #endregion Methods - Overriden
    }
}
