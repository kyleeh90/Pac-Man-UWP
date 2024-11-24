using Microsoft.Graphics.Canvas;
using Windows.Foundation;

namespace GameLibrary
{
    public class PowerPellet : Pellet
    {
        #region Constructors

        /// <summary>
        /// Constructs a new Pellet object.
        /// </summary>
        /// <param name="pixelX">The left edge of the object, in pixels.</param>
        /// <param name="pixelY">The top edge of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        public PowerPellet(double pixelX, double pixelY, Size size) : base(pixelX, pixelY, size)
        {
            Points = 50;
        }

        #endregion Constructors

        #region Overridden Methods

        public override void Draw(CanvasDrawingSession drawingSession)
        {
            if (Visible)
            {
                base.Draw(drawingSession);
            }
        }

        #endregion Overridden Methods
    }
}
