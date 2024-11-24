namespace GameLibrary
{
    /// <summary>
    /// Class for the ghosts idle state.
    /// </summary>
    public sealed class GhostIdle : GhostState
    {
        #region Constructors

        /// <summary>
        /// Creates an empty GhostIdle object.
        /// </summary>
        /// <param name="stateMachine">The state machine this state is attached to.</param>
        /// <param name="managedGhost">The ghost this state is managing.</param>
        public GhostIdle(GhostStateMachine stateMachine, Ghost managedGhost) : base(stateMachine, managedGhost, GhostStateType.Idle) { }

        #endregion Constructors
    }
}
