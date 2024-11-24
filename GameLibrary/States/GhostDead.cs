using Microsoft.Graphics.Canvas;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace GameLibrary
{
    public sealed class GhostDead : GhostState
    {
        #region Fields

        /// <summary>
        /// The timer for how long the score should be displayed.
        /// </summary>
        private int scoreDisplayTimer = 180;

        #endregion Fields

        #region Properties - Static

        /// <summary>
        /// Array of bitmaps for the score.
        /// </summary>
        private static CanvasBitmap[] ScoreBitmaps { get; set; }

        /// <summary>
        /// The sprite for the score.
        /// </summary>
        public static CanvasBitmap ScoreSprite { get; private set; }

        #endregion Properties - Static

        #region Constructors

        /// <summary>
        /// Creates a GhostDead object.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostDead(GhostStateMachine stateMachine, Ghost managedGhost) : base(stateMachine, managedGhost, GhostStateType.Dead) { }

        #endregion Constructors

        #region Methods - Static

        /// <summary>
        /// Load the resources for the score sprites.
        /// </summary>
        /// <param name="sender">The CanvasControl or AnimatedCanvasControl that is calling this method</param>
        public static async Task LoadResources(ICanvasResourceCreator sender) 
        {
            // Initialize the array
            ScoreBitmaps = new CanvasBitmap[4];

            // Load the sprites
            ScoreBitmaps[0] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Scores/200.png"));
            ScoreBitmaps[1] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Scores/400.png"));
            ScoreBitmaps[2] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Scores/800.png"));
            ScoreBitmaps[3] = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/Scores/1600.png"));

            // Set the sprite
            ScoreSprite = ScoreBitmaps[0];
        }

        #endregion Methods - Static

        #region Methods - Overriden

        public override void Enter(GhostStateType previousState)
        {
            // Fix a bug where the ghost will leave the map upwards when going home if they were leaving home before dying
            // This code is executed when leaving home is completed, so need to make sure it's run if the ghost dies before completion
            if (previousState == GhostStateType.LeavingHome)
            {
                // Ghosts always leave to the left
                ManagedGhost.DesiredDirection = Direction.Left;

                // Set the force reverse flag if the ghost is leaving right and set the previous position to the current position
                if (ManagedGhost.LeaveRight)
                {
                    ManagedGhost.PreviousGridPosition = ManagedGhost.GridPosition;
                    ManagedGhost.ForceReverse = true;
                }
            }

            // Get the current multiplier to grab the correct score sprite
            int index = Player.GhostMultiplier == 1 ? 
                0 :
                Player.GhostMultiplier == 8 ?
                3 :
                Player.GhostMultiplier / 2;
            ScoreSprite = ScoreBitmaps[index];

            // Set score display timer
            scoreDisplayTimer = 180;

            // Putting the media player in a using block makes it not play the sound properly.
            // So subscribe to the mediaended event and dispose it there.
            MediaPlayer deathPlayer = new MediaPlayer() 
            {
                Volume = 0.05,
                Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Sounds/eat_ghost.wav"))
            };

            deathPlayer.MediaEnded += (sender, e) =>
            {
                deathPlayer.Dispose();
                deathPlayer = null;
            };

            deathPlayer.Play();
        }

        public override void Update(double deltaTime, long updateCount, Point playerPosition, Direction playerDirection,
                                    Point blinkyPosition, bool isSecondUpdate = false)
        {
            // Decrement the timer
            if (scoreDisplayTimer > 0)
            {
                scoreDisplayTimer--;
            }
            else if (scoreDisplayTimer == 0)
            {
                StateMachine.SetState(GhostStateType.GoingHome);
            }
        }

        #endregion Methods - Overriden
    }
}
