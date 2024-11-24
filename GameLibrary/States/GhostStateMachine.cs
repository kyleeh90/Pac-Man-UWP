using Windows.Foundation;

namespace GameLibrary
{
    /// <summary>
    /// Enum for the different states a ghost can be in.
    /// </summary>
    public enum GhostStateType
    {
        Chase,
        Dead,
        EnteringHome,
        GoingHome,
        Home,
        Idle,
        LeavingHome,
        Scatter
    }

    public sealed class GhostStateMachine
    {
        #region Fields

        /// <summary>
        /// The Ghost object this state machine is attached to.
        /// </summary>
        private Ghost managedGhost;

        #endregion Fields

        #region Fields - States

        /// <summary>
        /// A reference to the currently running state.
        /// </summary>
        private GhostState currentState;

        /// <summary>
        /// The GhostChase state.
        /// </summary>
        private GhostChase ghostChase;

        /// <summary>
        /// The GhostDead state.
        /// </summary>
        private GhostDead ghostDead;

        /// <summary>
        /// The GhostEnteringHome state.
        /// </summary>
        private GhostEnteringHome ghostEnteringHome;

        /// <summary>
        /// The GhostGoingHome state.
        /// </summary>
        private GhostGoingHome ghostGoingHome;

        /// <summary>
        /// The GhostHome state.
        /// </summary>
        private GhostHome ghostHome;

        /// <summary>
        /// The GhostIdle state.
        /// </summary>
        /// <remarks>Completely empty state used when Ghost shouldn't be doing anything.</remarks>
        private GhostIdle ghostIdle;

        /// <summary>
        /// The GhostLeavingHome state.
        /// </summary>
        private GhostLeavingHome ghostLeavingHome;

        /// <summary>
        /// The GhostScatter state.
        /// </summary>
        private GhostScatter ghostScatter;

        #endregion Fields - States

        #region Fields - Timing

        /// <summary>
        /// The timer for the chase mode.
        /// </summary>
        private int chaseTimer = 0;

        /// <summary>
        /// The timer for the scatter mode.
        /// </summary>
        private int scatterTimer = 0;

        /// <summary>
        /// How many times the ghosts have been in chase mode.
        /// </summary>
        private int stateChanges = 0;

        #endregion Fields - Timing

        #region Properties

        /// <summary>
        /// The next state type that will be transitioned to.
        /// </summary>
        public GhostStateType NextStateType { get; private set; } = GhostStateType.Scatter;

        public GhostStateType PreviousStateType { get; private set; } = GhostStateType.Idle;

        /// <summary>
        /// The type of state currently being executed.
        /// </summary>
        public GhostStateType StateType { get; private set; } = GhostStateType.Idle;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor for the ghost state machine.
        /// </summary>
        /// <param name="ghost">The Ghost object this state machine is attached to.</param>
        public GhostStateMachine(Ghost ghost)
        {
            // Set the ghost
            managedGhost = ghost;

            // Create state references
            ghostChase = new GhostChase(this, ghost);
            ghostDead = new GhostDead(this, ghost);
            ghostEnteringHome = new GhostEnteringHome(this, ghost);
            ghostGoingHome = new GhostGoingHome(this, ghost);
            ghostHome = new GhostHome(this, ghost);
            ghostIdle = new GhostIdle(this, ghost);
            ghostLeavingHome = new GhostLeavingHome(this, ghost);
            ghostScatter = new GhostScatter(this, ghost);

            // Set the state to idle to begin
            currentState = ghostIdle;
            StateType = GhostStateType.Idle;
        }

        #endregion Constructors

        #region Methods - Public

        /// <summary>
        /// Sets the state of the ghost.
        /// </summary>
        /// <param name="newState"></param>
        public void SetState(GhostStateType newState) 
        {
            // Reset the timers
            if (StateType == GhostStateType.Idle) 
            {
                // Reset timers
                chaseTimer = Ghost.StateTimes[1];
                scatterTimer = Ghost.StateTimes[0];

                // Reset state changes
                stateChanges = 0;

                NextStateType = GhostStateType.Scatter;
            }

            // Set the new state.
            switch (newState)
            {
                case GhostStateType.Chase:
                    currentState = ghostChase;
                    break;
                case GhostStateType.Dead:
                    currentState = ghostDead;
                    break;
                case GhostStateType.EnteringHome:
                    currentState = ghostEnteringHome;
                    break;
                case GhostStateType.GoingHome:
                    currentState = ghostGoingHome;
                    break;
                case GhostStateType.Home:
                    currentState = ghostHome;
                    break;
                case GhostStateType.Idle:
                    currentState = ghostIdle;
                    break;
                case GhostStateType.LeavingHome:
                    currentState = ghostLeavingHome;
                    break;
                case GhostStateType.Scatter:
                    currentState = ghostScatter;
                    break;
            }

            // Set the StateType to the new state
            PreviousStateType = StateType;
            StateType = newState;

            // Enter the new state.
            currentState.Enter(PreviousStateType);
        }

        /// <summary>
        /// Updates the ghost.
        /// </summary>
        /// <param name="deltaTime">The time the last frame took to complete, in seconds.</param>
        /// <param name="updateCount">The amount of updates the game has had.</param>
        /// <param name="playerPosition">The position of the player, in grid units.</param>
        /// <param name="playerDirection">The direction the player is facing.</param>
        /// <param name="blinkyPosition">The position of the Blinky ghost, in grid units.</param>
        /// <param name="isSecondUpdate">Is this the first or second update of this frame.</param>
        public void Update(double deltaTime, long updateCount, Point playerPosition, Direction playerDirection,
                           Point blinkyPosition, bool isSecondUpdate = false)
        {
            // Update the state if it's not idle
            if (StateType != GhostStateType.Idle)
            {
                // Don't update the ghosts if they're frozen
                if (managedGhost.FreezeFramesRemaining > 0)
                {
                    managedGhost.FreezeFramesRemaining--;

                    // Return if the ghost is not dead, going home, or entering home
                    if (StateType != GhostStateType.Dead && StateType != GhostStateType.GoingHome && 
                        StateType != GhostStateType.EnteringHome)
                    {
                        return;
                    }  
                }

                // Animate the body every 8 frames
                if (isSecondUpdate && updateCount % 8 == 0)
                {
                    managedGhost.AnimateBody();
                }

                // Update state timers
                #region State Timers

                if (!managedGhost.IsFrightened && isSecondUpdate && stateChanges <= 7)
                {
                    // If it's the 8th state change, set mode to chase
                    if (stateChanges == 7)
                    {
                        if (StateType == GhostStateType.Chase || StateType == GhostStateType.Scatter)
                        {
                            SetState(GhostStateType.Chase);
                        }
                        else
                        {
                            managedGhost.LeaveRight = true;
                        }
                        NextStateType = GhostStateType.Chase;
                        stateChanges++;
                    }
                    else
                    {
                        // Decrement the timer
                        if (stateChanges % 2 != 0)
                        {
                            chaseTimer--;
                        }
                        else
                        {
                            scatterTimer--;
                        }

                        // Check the timer conditions
                        if (chaseTimer == 0)
                        {
                            if (StateType == GhostStateType.Chase || StateType == GhostStateType.Scatter)
                            {
                                SetState(GhostStateType.Scatter);
                            }
                            else
                            {
                                managedGhost.LeaveRight = true;
                            }
                            chaseTimer = Ghost.StateTimes[stateChanges];
                            NextStateType = GhostStateType.Scatter;
                            stateChanges++;
                        }

                        else if (scatterTimer == 0)
                        {
                            if (StateType == GhostStateType.Chase || StateType == GhostStateType.Scatter)
                            {
                                SetState(GhostStateType.Chase);
                            }
                            else if (StateType == GhostStateType.Home || StateType == GhostStateType.LeavingHome)
                            {
                                managedGhost.LeaveRight = true;
                            }
                            scatterTimer = Ghost.StateTimes[stateChanges];
                            NextStateType = GhostStateType.Chase;
                            stateChanges++;
                        }
                    }
                }

                #endregion State Timers

                // Reverse if the flag is set at the next tile
                // Don't reverse on teleport tile as the ghost will glitch out of the map
                if (managedGhost.ForceReverse && !managedGhost.PreviousGridPosition.Equals(managedGhost.GridPosition))
                {
                    if (managedGhost.CurrentTileType == TileType.Teleport)
                    {
                        return;
                    }

                    managedGhost.CurrentDirection = managedGhost.CurrentDirection.Reversed();
                    managedGhost.DesiredDirection = managedGhost.CurrentDirection;
                    managedGhost.SetSpriteDirection();
                    managedGhost.ForceReverse = false;

                    return;
                }

                // Check the frightened timer and handle animating the frightened state
                if (isSecondUpdate && managedGhost.IsFrightened && MovingEntity.FrightenedTimer == 0)
                {
                    managedGhost.IsFrightened = false;
                    managedGhost.DrawFrightenedWhite = false;
                }
                // Switch between drawing blue and white every 14 updates if frightened
                else if (isSecondUpdate && managedGhost.IsFrightened &&
                         MovingEntity.FrightenedTimer <= MovingEntity.FrightenedData.Item2 &&
                         updateCount % 14 == 0) 
                {
                    managedGhost.DrawFrightenedWhite = !managedGhost.DrawFrightenedWhite;
                }

                // Update the current state
                if (managedGhost.IsFrightened &&
                    StateType != GhostStateType.Home && 
                    StateType != GhostStateType.LeavingHome)
                {
                    FrightenedUpdate(deltaTime);
                }
                else 
                {
                    currentState.Update(deltaTime, updateCount, playerPosition, playerDirection, blinkyPosition, isSecondUpdate);
                }
            }
        }

        #endregion Methods - Public

        #region Methods - Private

        /// <summary>
        /// Loop to run if the ghost is in the frightened state.
        /// </summary>
        /// <param name="deltaTime">The time the last frame took to complete, in seconds.</param>
        private void FrightenedUpdate(double deltaTime)
        {
            // Update the tile types and the speed
            currentState.UpdateTileTypes();
            currentState.UpdateSpeed();

            // Check if the next tile is a restricted tile or an intersection the ghost needs to turn at
            if ((managedGhost.NextTileType == TileType.Restricted || managedGhost.NextTileType == TileType.Intersection) &&
                managedGhost.PreviousGridPosition != managedGhost.GridPosition)
            {
                managedGhost.DesiredDirection = managedGhost.GetBestDirection();
            }

            // If the ghost will move, move it.
            if (managedGhost.Speed == 1)
            {
                managedGhost.Move(managedGhost.Speed);
            }
        }

        #endregion Methods - Private
    }
}
