using System;
using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// Class for the ghosts home state.
    /// </summary>
    public sealed class GhostHome : GhostState
    {
        #region Fields

        /// <summary>
        /// Indicates whether the ghost is moving up or down in the house.
        /// </summary>
        private bool isMovingUp = true;

        /// <summary>
        /// The top and bottom of the ghost house.
        /// </summary>
        private double bottomOfHome = 148, topOfHome = 139;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates an empty GhostHome object.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostHome(GhostStateMachine stateMachine, Ghost managedGhost) : base(stateMachine, managedGhost, GhostStateType.Home) { }

        #endregion Constructors

        #region Methods - Overriden

        public override void Enter(GhostStateType previousState)
        {
            // If it's Blinky, they immediately leave the house
            if (ManagedGhost.GhostType == GhostType.Blinky)
            {
                StateMachine.SetState(GhostStateType.LeavingHome);
            }
        }

        public override void Update(double deltaTime, long updateCount, Point playerPosition, Direction playerDirection, 
                                    Point blinkyPosition, bool isSecondUpdate = false)
        {
            // Update speed and types
            UpdateSpeed();

            // If the ghost is Clyde, Inky, or Pinky they will move up and down in the house
            if (ManagedGhost.GhostType != GhostType.Blinky) 
            {
                // Check if at the top or bottom of the house
                if (Math.Floor(ManagedGhost.CenterY) == topOfHome) 
                {
                    ManagedGhost.DesiredDirection = Direction.Down;
                    ManagedGhost.SetSpriteDirection();
                    isMovingUp = false;
                }
                else if (Math.Floor(ManagedGhost.CenterY) == bottomOfHome)
                {
                    ManagedGhost.DesiredDirection = Direction.Up;
                    ManagedGhost.SetSpriteDirection();
                    isMovingUp = true;
                }

                // Move the ghost up or down
                ManagedGhost.MoveTowardsY(ManagedGhost.Speed, isMovingUp ? topOfHome : bottomOfHome);
            }
        }

        #endregion Methods - Overriden
    }
}