namespace GameLibrary
{
    public static class RenderProperties
    {
        /// <summary>
        /// The aspect ratio of the game
        /// </summary>
        public const double ASPECT_RATIO = 224.0 / 288.0;

        /// <summary>
        /// The horizontal center of the window
        /// </summary>
        public static double CenterX => (WindowWidth - TargetWidth) / 2;

        /// <summary>
        /// The vertical center of the window
        /// </summary>
        public static double CenterY => (WindowHeight - TargetHeight) / 2;

        /// <summary>
        /// The target width of the render
        /// </summary>
        public static double TargetWidth { get; set; } = 224;

        /// <summary>
        /// The target height of the render
        /// </summary>
        public static double TargetHeight { get; set; } = 288;

        /// <summary>
        /// The width of the window
        /// </summary>
        public static double WindowWidth { get; set; } = 224;

        /// <summary>
        /// The height of the window
        /// </summary>
        public static double WindowHeight { get; set; } = 288;


        /// <summary>
        /// Update the target size based on the window size
        /// </summary>
        public static void UpdateTargetSize()
        {
            if (WindowWidth / WindowHeight > ASPECT_RATIO)
            {
                TargetHeight = WindowHeight;
                TargetWidth = TargetHeight * ASPECT_RATIO;
            }
            else
            {
                TargetWidth = WindowWidth;
                TargetHeight = TargetWidth / ASPECT_RATIO;
            }
        }
    }
}
