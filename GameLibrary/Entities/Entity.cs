using Microsoft.Graphics.Canvas;
using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// A base class for all objects that can be drawn on the screen.
    /// </summary>
    public class Entity
    {
        #region Fields

        protected double pixelX, pixelY;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The left edge of the object, in pixels.
        /// </summary>
        public virtual double PixelX 
        {
            get => pixelX;
            protected set => pixelX = value;
        }

        /// <summary>
        /// The top edge of the object, in pixels.
        /// </summary>
        public virtual double PixelY 
        {
            get => pixelY;
            protected set => pixelY = value;
        }

        /// <summary>
        /// The size of the object, in pixels.
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// The CanvasBitmap to draw.
        /// </summary>
        public CanvasBitmap Sprite { get; set; }

        /// <summary>
        /// Whether or not the entity should be drawn.
        /// </summary>
        public bool Visible { get; set; } = true;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructs a new Entity object.
        /// </summary>
        /// <param name="pixelX">The left edge of the object, in pixels.</param>
        /// <param name="pixelY">The top edge of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        public Entity(double pixelX, double pixelY, Size size)
        {
            PixelX = pixelX;
            PixelY = pixelY;
            Size = size;
        }

        #endregion Constructors

        #region Methods - Virtual

        /// <summary>
        /// Draws the object.
        /// </summary>
        /// <param name="drawingSession">CanvasDrawingSession to draw on.</param>
        public virtual void Draw(CanvasDrawingSession drawingSession) 
        {
            drawingSession.DrawImage(Sprite, new Rect(PixelX, PixelY, Size.Width, Size.Height));
        }

        #endregion Methods - Virtual
    }
}
