using Windows.Foundation;

namespace GameLibrary
{
    public sealed class GhostChase : GhostState
    {
        #region Constructors

        /// <summary>
        /// Creates a GhostChase object.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostChase(GhostStateMachine stateMachine, Ghost managedGhost) : base(stateMachine, managedGhost, GhostStateType.Chase) { }

        #endregion Constructors

        #region Methods - Overriden

        public override void Enter(GhostStateType previousState)
        {
            if (previousState == GhostStateType.Scatter)
            {
                // Force reverse if leaving scatter mode
                ManagedGhost.ForceReverse = true;
            }

            // Set the target tile
            ManagedGhost.TargetTile = ManagedGhost.ScatterTarget;
        }

        public override void Update(double deltaTime, long updateCount, Point playerPosition, Direction playerDirection,
                                    Point blinkyPosition, bool isSecondUpdate = false)
        {
            // Update the tile types and the speed
            UpdateTileTypes();
            UpdateSpeed();

            // Set the target tile
            switch (ManagedGhost.GhostType)
            {
                case GhostType.Blinky:
                case GhostType.Clyde:
                    ManagedGhost.SetChaseTarget(playerPosition);
                    break;
                case GhostType.Inky:
                    ManagedGhost.SetChaseTarget(playerPosition, playerDirection, blinkyPosition);
                    break;
                case GhostType.Pinky:
                    ManagedGhost.SetChaseTarget(playerPosition, playerDirection);
                    break;
            }

            // Check if the next tile is a restricted tile or an intersection the ghost needs to turn at
            if ((ManagedGhost.NextTileType == TileType.Restricted || ManagedGhost.NextTileType == TileType.Intersection) &&
                ManagedGhost.PreviousGridPosition != ManagedGhost.GridPosition)
            {
                ManagedGhost.DesiredDirection = ManagedGhost.GetBestDirection();
            }

            // If the ghost will move, move it.
            if (ManagedGhost.Speed == 1)
            {
                ManagedGhost.Move(ManagedGhost.Speed);
            }
        }

        #endregion Methods - Overriden
    }
}