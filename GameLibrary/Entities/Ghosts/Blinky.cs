using Microsoft.Graphics.Canvas;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// Class for the Blinky ghost.
    /// </summary>
    public sealed class Blinky : Ghost, IResourceLoader
    {
        #region Constructors

        /// <summary>
        /// Constructs a new Clyde object.
        /// </summary>
        /// <param name="centerX">The center of the object, in pixels.</param>
        /// <param name="centerY">The center of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        public Blinky(double centerX, double centerY, Size size) : base(centerX, centerY, size, GhostType.Blinky) 
        {
            // Set the scatter target
            ScatterTarget = new Point(25, 0);
        }

        #endregion Constructors

        #region Methods - Implemented

        public async Task LoadResources(ICanvasResourceCreator sender)
        {
            AnimationFrames = new CanvasBitmap[2];

            AnimationFrames[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Blinky/body_1.png"));
            AnimationFrames[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Ghosts/Blinky/body_2.png"));

            // Set the sprite
            Sprite = AnimationFrames[0];

            // Set the direction and eyes
            CurrentDirection = Direction.Left;
            DesiredDirection = Direction.Left;
            SetSpriteDirection();
        }

        #endregion Methods - Implemented

        #region Methods - Overriden

        public override void Reset()
        {
            base.Reset();

            CurrentDirection = Direction.Left;
            DesiredDirection = Direction.Left;

            SetSpriteDirection();
        }

        #endregion Methods - Overriden
    }
}
