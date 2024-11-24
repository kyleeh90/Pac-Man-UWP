using Microsoft.Graphics.Canvas;
using System.Threading.Tasks;

namespace GameLibrary
{
    /// <summary>
    /// Interface for loading resources via ICanvasResourceCreator.
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        /// Loads the resources needed for drawing.
        /// </summary>
        /// <param name="sender">The CanvasControl or AnimatedCanvasControl that is calling this method</param>
        /// <remarks>Must be overriden as async</remarks>
        Task LoadResources(ICanvasResourceCreator sender);
    }
}
