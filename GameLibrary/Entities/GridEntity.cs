using System;
using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// An inherited class that represents an Entity that is part of a grid.
    /// </summary>
    public class GridEntity : Entity
    {
        #region Fields

        // Backing fields for properties
        protected Point gridPosition;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The position of the object on the grid.
        /// </summary>
        public Point GridPosition => gridPosition;

        public override double PixelX
        {
            get => pixelX;
            protected set
            {
                pixelX = value;
                gridPosition.X = Math.Floor(pixelX / Constants.TILE_SIZE);
            }
        }

        public override double PixelY
        {
            get => pixelY;
            protected set
            {
                pixelY = value;
                gridPosition.Y = Math.Floor(pixelY / Constants.TILE_SIZE);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructs a new GridEntity object.
        /// </summary>
        /// <param name="pixelX">The left edge of the object, in pixels.</param>
        /// <param name="pixelY">The top edge of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        public GridEntity(double pixelX, double pixelY, Size size) : base(pixelX, pixelY, size) 
        {
            PixelX = pixelX;
            PixelY = pixelY;
        }

        #endregion Constructors
    }
}
