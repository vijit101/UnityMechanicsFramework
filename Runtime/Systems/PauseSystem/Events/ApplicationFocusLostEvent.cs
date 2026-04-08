namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Fired by PauseSystem_UMFOSS when the application loses focus (alt-tab, app switch).
    /// Fired before auto-pause triggers (if toggleOnFocusLoss is enabled).
    /// Can be used by analytics systems to track focus-loss events.
    /// </summary>
    public struct ApplicationFocusLostEvent { }
}
