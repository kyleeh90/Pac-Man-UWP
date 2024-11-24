using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GameLibrary
{
    public sealed class Pinky : Ghost, IResourceLoader
    {
        #region Constructors

        /// <summary>
        /// Constructs a new Clyde object.
        /// </summary>
        /// <param name="centerX">The center of the object, in pixels.</param>
        /// <param name="centerY">The center of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        public Pinky(double centerX, double centerY, Size size) : base(centerX, centerY, size, GhostType.Pinky) 
        {
            // Set the scatter target
            ScatterTarget = new Point(2, 0);
        }

        #endregion Constructors

        #region Methods - Implemented

        public async Task LoadResources(ICanvasResourceCreator sender)
        {
            AnimationFrames = new CanvasBitmap[2];

            AnimationFrames[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Pinky/body_1.png"));
            AnimationFrames[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Pinky/body_2.png"));

            // Set the sprite
            Sprite = AnimationFrames[0];

            // Set the direction and eyes
            CurrentDirection = Direction.Down;
            DesiredDirection = Direction.Down;
            SetSpriteDirection();
        }

        #endregion Methods - Implemented

        #region Methods - Overriden

        public override void Reset()
        {
            base.Reset();

            CurrentDirection = Direction.Down;
            DesiredDirection = Direction.Down;

            SetSpriteDirection();
        }

        public override void SetChaseTarget(Point playerPosition, Direction playerFacing)
        {
            // The target is 4 tiles ahead of the player in the direction they are facing
            // Like Inky if they're facing up the offset also goes to the left by 4
            switch (playerFacing)
            {
                case Direction.Up:
                    TargetTile = new Point(playerPosition.X - 4, playerPosition.Y - 4);
                    break;
                case Direction.Down:
                    TargetTile = new Point(playerPosition.X, playerPosition.Y + 4);
                    break;
                case Direction.Left:
                    TargetTile = new Point(playerPosition.X - 4, playerPosition.Y);
                    break;
                case Direction.Right:
                    TargetTile = new Point(playerPosition.X + 4, playerPosition.Y);
                    break;
            }
        }

        #endregion Methods - Overriden
    }
}
