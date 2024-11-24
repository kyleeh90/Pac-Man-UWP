using Microsoft.Graphics.Canvas;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GameLibrary
{
    public sealed class Inky : Ghost, IResourceLoader
    {
        #region Constructors

        /// <summary>
        /// Constructs a new Clyde object.
        /// </summary>
        /// <param name="centerX">The center of the object, in pixels.</param>
        /// <param name="centerY">The center of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        public Inky(double centerX, double centerY, Size size) : base(centerX, centerY, size, GhostType.Inky) 
        {
            // Set the scatter target
            ScatterTarget = new Point(27, 34);

            // Set the pellet limit for the first level as it's 0 for every level afterwards for Inky
            if (GameManager.CurrentLevel == 1) 
            {
                PelletLimit = 30;
            }
        }

        #endregion Constructors

        #region Methods - Implemented

        public async Task LoadResources(ICanvasResourceCreator sender)
        {
            AnimationFrames = new CanvasBitmap[2];

            AnimationFrames[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Inky/body_1.png"));
            AnimationFrames[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Inky/body_2.png"));

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

        public override void SetChaseTarget(Point playerPosition, Direction playerFacing, Point blinkyPosition)
        {
            // Get a Point 2 tiles in front of the player
            // If the player is facing up, it's also offset to the left by 2.
            Point target = playerPosition;

            switch (playerFacing)
            {
                case Direction.Up:
                    target.Y -= 2;
                    target.X -= 2;
                    break;
                case Direction.Down:
                    target.Y += 2;
                    break;
                case Direction.Left:
                    target.X -= 2;
                    break;
                case Direction.Right:
                    target.X += 2;
                    break;
            }

            // Get the positions of Blinky relative to the target and apply them to the target
            target.X += Math.Abs(target.X - blinkyPosition.X) * Math.Sign(target.X - blinkyPosition.X);
            target.Y += Math.Abs(target.Y - blinkyPosition.Y) * Math.Sign(target.Y - blinkyPosition.Y);

            // Set the target tile
            TargetTile = target;
        }

        #endregion Methods - Overriden
    }
}
