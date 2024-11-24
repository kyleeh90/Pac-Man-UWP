using System;
using Windows.Foundation;

namespace GameLibrary
{
    public sealed class GhostEnteringHome : GhostState
    {
        #region Constructors

        /// <summary>
        /// Creates a GhostEnteringHome object.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostEnteringHome(GhostStateMachine stateMachine, Ghost managedGhost) : base(stateMachine, managedGhost, GhostStateType.EnteringHome) { }

        #endregion Constructors

        #region Methods - Overriden

        public override void Update(double deltaTime, long updateCount, Point playerPosition, Direction playerDirection,
                                    Point blinkyPosition, bool isSecondUpdate = false)
        {
            // Update the speed
            UpdateSpeed();

            // Blinky and Pinky move straight down only
            if (ManagedGhost.GhostType == GhostType.Blinky || ManagedGhost.GhostType == GhostType.Pinky)
            {
                if (Math.Floor(ManagedGhost.CenterY) != 143)
                {
                    ManagedGhost.MoveTowardsY(ManagedGhost.Speed, 143);
                }
                else 
                {
                    // Set the ghost to the home state
                    StateMachine.SetState(GhostStateType.Home);
                }
            }
            // Clyde and Inky need to move to their original X position first
            else if (ManagedGhost.GhostType == GhostType.Clyde || ManagedGhost.GhostType == GhostType.Inky)
            {
                if (Math.Floor(ManagedGhost.CenterY) != ManagedGhost.InitialPosition.Y)
                {
                    ManagedGhost.MoveTowardsY(ManagedGhost.Speed, ManagedGhost.InitialPosition.Y);
                }
                else if (Math.Floor(ManagedGhost.CenterX) != ManagedGhost.InitialPosition.X)
                {
                    ManagedGhost.MoveTowardsX(ManagedGhost.Speed, ManagedGhost.InitialPosition.X);
                }
                else
                {
                    // Set the ghost to the home state
                    StateMachine.SetState(GhostStateType.Home);
                }
            }
        }

        #endregion Methods - Overriden
    }
}
