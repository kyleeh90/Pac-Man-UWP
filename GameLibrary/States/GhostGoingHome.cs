using System;
using Windows.Foundation;

namespace GameLibrary
{
    public sealed class GhostGoingHome : GhostState
    {
        #region Constructors

        /// <summary>
        /// Creates a GhostGoingHome object.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostGoingHome(GhostStateMachine stateMachine, Ghost managedGhost) : base(stateMachine, managedGhost, GhostStateType.GoingHome) { }

        #endregion Constructors

        #region Methods - Overriden

        public override void Enter(GhostStateType previousState)
        {
            // Set the target tile to the left side of the ghost house
            ManagedGhost.TargetTile = new Point(13, 14);
        }

        public override void Update(double deltaTime, long updateCount, Point playerPosition, Direction playerDirection,
                                    Point blinkyPosition, bool isSecondUpdate = false)
        {
            // Update the tile types and the speed
            UpdateTileTypes();
            UpdateSpeed();

            // Check if the next tile is a restricted tile or an intersection the ghost needs to turn at
            if ((ManagedGhost.NextTileType == TileType.Restricted || ManagedGhost.NextTileType == TileType.Intersection) &&
                ManagedGhost.PreviousGridPosition != ManagedGhost.GridPosition)
            {
                ManagedGhost.DesiredDirection = ManagedGhost.GetBestDirection();
            }

            ManagedGhost.Move(ManagedGhost.Speed);

            // Check if the ghost is lined up with the entrance
            if (Math.Floor(ManagedGhost.CenterX) == Constants.HOUSE_EXIT_X &&
                Math.Floor(ManagedGhost.CenterY) == Constants.HOUSE_EXIT_Y)
            {
                // Set the ghost to the entering home state
                StateMachine.SetState(GhostStateType.EnteringHome);
            }
        }

        #endregion Methods - Overriden
    }
}
