namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Fired by PauseSystem_UMFOSS when the game resumes.
    ///
    /// Subscribers:
    ///   PlayerController  — re-enable movement input
    ///   EnemyFSM          — resume AI tick
    ///   PauseMenuUI       — hide pause panel
    /// </summary>
    public struct GameResumedEvent
    {
        /// <summary>The Time.timeScale restored on resume (same value that was stored when paused).</summary>
        public float restoredTimeScale;
    }
}
