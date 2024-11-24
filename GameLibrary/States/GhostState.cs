using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// Base class for every ghost state.
    /// </summary>
    public abstract class GhostState
    {
        #region Properties

        /// <summary>
        /// The Ghost object this state is managing.
        /// </summary>
        public Ghost ManagedGhost { get; private set; }

        /// <summary>
        /// The state machine this state is attached to.
        /// </summary>
        public GhostStateMachine StateMachine { get; private set; }

        /// <summary>
        /// The type of this state.
        /// </summary>
        public GhostStateType StateType { get; private set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructs a new ghost state.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostState(GhostStateMachine stateMachine, Ghost managedGhost, GhostStateType stateType)
        {
            // Set the state machine.
            StateMachine = stateMachine;

            // Set the ghost.
            ManagedGhost = managedGhost;

            // Set the state type
            StateType = stateType;
        }

        #endregion Constructors

        #region Methods - Public

        /// <summary>
        /// Update the speed of the ghost based on the current tile type.
        /// </summary>
        public void UpdateSpeed() 
        {
            // If the ghost is home or leaving home, they use tunnel speed
            if (ManagedGhost.CurrentState == GhostStateType.Home || ManagedGhost.CurrentState == GhostStateType.LeavingHome)
            {
                ManagedGhost.Speed = ManagedGhost.SpeedData[1].GetBit(16);
                ManagedGhost.SpeedData[1] = ManagedGhost.SpeedData[1].BitRotateRight(1);
                return;
            }
            // If they're going home or entering home they always move one pixel
            else if (ManagedGhost.CurrentState == GhostStateType.GoingHome || ManagedGhost.CurrentState == GhostStateType.EnteringHome)
            {
                ManagedGhost.Speed = 1;
                return;
            }

            switch (ManagedGhost.CurrentTileType)
            {
                case TileType.Tunnel:
                    ManagedGhost.Speed = ManagedGhost.SpeedData[1].GetBit(16);
                    ManagedGhost.SpeedData[1] = ManagedGhost.SpeedData[1].BitRotateRight(1);
                    break;
                default:
                    if (ManagedGhost.IsFrightened) 
                    {
                        ManagedGhost.Speed = ManagedGhost.SpeedData[2].GetBit(16);
                        ManagedGhost.SpeedData[2] = ManagedGhost.SpeedData[2].BitRotateRight(1);
                    }
                    else
                    {
                        ManagedGhost.Speed = ManagedGhost.SpeedData[0].GetBit(16);
                        ManagedGhost.SpeedData[0] = ManagedGhost.SpeedData[0].BitRotateRight(1);
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates the current and next tile types of the ghost.
        /// </summary>
        public void UpdateTileTypes() 
        {
            // Get the current tile type
            ManagedGhost.CurrentTileType = Map.GetTileAt(ManagedGhost.GridPosition);

            // Get the next tile type
            ManagedGhost.NextTileType = Map.GetNextTile(ManagedGhost.CurrentDirection, ManagedGhost.GridPosition);
        }

        #endregion Methods - Public

        #region Methods - Virtual

        /// <summary>
        /// Enters the state and performs any necessary setup.
        /// </summary>
        /// <param name="previousState">The previous state of the state machine.</param>
        public virtual void Enter(GhostStateType previousState){ }

        /// <summary>
        /// Updates the ghost.
        /// </summary>
        /// <param name="deltaTime">The time the last frame took to complete, in seconds.</param>
        /// <param name="updateCount">The amount of updates the game has had.</param>
        /// <param name="playerPosition">The position of the player, in grid units.</param>
        /// <param name="playerDirection">The direction the player is facing.</param>
        /// <param name="blinkyPosition">The position of the Blinky ghost, in grid units.</param>
        /// <param name="isSecondUpdate">Is this the second update of this frame?</param>
        public virtual void Update(double deltaTime, long updateCount, Point playerPosition, Direction playerDirection,
                           Point blinkyPosition, bool isSecondUpdate = false)
        { }

        #endregion Methods - Virtual
    }
}
