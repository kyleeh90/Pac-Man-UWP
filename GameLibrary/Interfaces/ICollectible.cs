namespace GameLibrary
{
    /// <summary>
    /// Interface for objects that can be collected.
    /// </summary>
    public interface ICollectible
    {
        /// <summary>
        /// The number of points the object is worth.
        /// </summary>
        int Points { get; }
    }
}
