namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Fired by PauseSystem_UMFOSS when the game pauses.
    ///
    /// Subscribers:
    ///   PlayerController  — disable movement input
    ///   EnemyFSM          — freeze AI tick
    ///   TimerUtility      — set internal pause flag
    ///   PauseMenuUI       — show pause panel
    ///   SceneManager      — block scene loads while paused
    /// </summary>
    public struct GamePausedEvent
    {
        /// <summary>The Time.timeScale that was active before pausing (may not be 1.0 — could be bullet time).</summary>
        public float previousTimeScale;
    }
}
