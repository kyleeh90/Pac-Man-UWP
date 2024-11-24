using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GameLibrary
{
    public sealed class GhostScatter : GhostState
    {
        #region Constructors

        /// <summary>
        /// Creates a GhostScatter object.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostScatter(GhostStateMachine stateMachine, Ghost managedGhost) : base(stateMachine, managedGhost, GhostStateType.Scatter) { }

        #endregion Constructors

        #region Methods - Overriden

        public override void Enter(GhostStateType previousState)
        {
            // If the previous state was chase, force reverse
            if (previousState == GhostStateType.Chase)
            {
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
