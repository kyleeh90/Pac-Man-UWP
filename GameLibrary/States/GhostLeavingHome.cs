using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GameLibrary
{
    public sealed class GhostLeavingHome : GhostState
    {
        #region Constructors

        /// <summary>
        /// Creates a GhostLeavingHome object.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostLeavingHome(GhostStateMachine stateMachine, Ghost managedGhost) : base(stateMachine, managedGhost, GhostStateType.LeavingHome) { }

        #endregion Constructors

        #region Methods - Overriden

        public override void Enter(GhostStateType previousState)
        {
            // Set sprite direction based on the ghost type
            if (ManagedGhost.GhostType == GhostType.Blinky || ManagedGhost.GhostType == GhostType.Pinky)
            {
                ManagedGhost.DesiredDirection = Direction.Up;
            }
            else if (ManagedGhost.GhostType == GhostType.Clyde)
            {
                ManagedGhost.DesiredDirection = Direction.Left;
            }
            else 
            {
                ManagedGhost.DesiredDirection = Direction.Right;
            }

            ManagedGhost.SetSpriteDirection();
        }

        public override void Update(double deltaTime, long updateCount, Point playerPosition, Direction playerDirection,
                                    Point blinkyPosition, bool isSecondUpdate = false)
        {
            // Update speed
            UpdateSpeed();

            // If speed is 0, don't move
            if (ManagedGhost.Speed == 0)
            {
                return;
            }

            // Move towards the exit
            if (Math.Floor(ManagedGhost.CenterX) != Constants.HOUSE_EXIT_X)
            {
                ManagedGhost.DesiredDirection = ManagedGhost.GhostType == GhostType.Inky ? Direction.Right : Direction.Left;

                ManagedGhost.MoveTowardsX(ManagedGhost.Speed, Constants.HOUSE_EXIT_X);
            }
            else if (Math.Floor(ManagedGhost.CenterY) != Constants.HOUSE_EXIT_Y)
            {
                ManagedGhost.DesiredDirection = Direction.Up;

                ManagedGhost.MoveTowardsY(ManagedGhost.Speed, Constants.HOUSE_EXIT_Y);
            }
            else 
            {
                // Ghosts always leave to the left
                ManagedGhost.DesiredDirection = Direction.Left;

                // Set the force reverse flag if the ghost is leaving right and set the previous position to the current position
                if (ManagedGhost.LeaveRight) 
                {
                    ManagedGhost.PreviousGridPosition = ManagedGhost.GridPosition;
                    ManagedGhost.ForceReverse = true;
                }

                // Set the state to the next state type
                StateMachine.SetState(StateMachine.NextStateType);
            }

            // Set the current direction to the desired direction
            ManagedGhost.CurrentDirection = ManagedGhost.DesiredDirection;

            // Set the sprite
            ManagedGhost.SetSpriteDirection();
        }

        #endregion Methods - Overriden
    }
}
