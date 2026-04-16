namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// Implemented by pooled objects that need lifecycle callbacks when borrowed or returned.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}
