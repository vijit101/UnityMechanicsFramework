namespace GameplayMechanicsUMFOSS.Core
{
    public interface ISaveable_UMFOSS
    {
        string GetSaveID();
        object CaptureState();
        void RestoreState(object state);
    }
}
