using Windows.Foundation;

namespace GameLibrary
{
    public class Pellet : GridEntity, ICollectible
    {
        #region Properties

        public int Points { get; protected set; } = 10;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructs a new Pellet object.
        /// </summary>
        /// <param name="pixelX">The left edge of the object, in pixels.</param>
        /// <param name="pixelY">The top edge of the object, in pixels.</param>
        /// <param name="size">The size of the object, in pixels.</param>
        public Pellet(double pixelX, double pixelY, Size size) : base(pixelX, pixelY, size) { }

        #endregion Constructors
    }
}
