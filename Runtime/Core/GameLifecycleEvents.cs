namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Fired when pause state changes. <see cref="IsPaused"/> true = paused, false = resumed.
    /// </summary>
    public struct GamePausedEvent
    {
        public bool IsPaused;
    }
}
