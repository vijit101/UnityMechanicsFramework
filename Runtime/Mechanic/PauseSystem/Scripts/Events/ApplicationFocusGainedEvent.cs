namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Fired by PauseSystem_UMFOSS when the application regains focus.
    /// Note: PauseSystem deliberately does NOT auto-resume on focus return.
    /// The player must manually press the pause key to resume.
    /// </summary>
    public struct ApplicationFocusGainedEvent { }
}
