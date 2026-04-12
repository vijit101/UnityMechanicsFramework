namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Contract for systems that expose snapshot state for a save/load layer.
    /// </summary>
    public interface ISaveable_UMFOSS
    {
        object CaptureState();
        void RestoreState(object state);
    }
}
