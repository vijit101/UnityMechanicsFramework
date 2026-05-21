namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>Fired whenever the slow motion resource changes.</summary>
    public struct BulletTimeResourceChangedEvent
    {
        public float current;
        public float max;
        public float percent;
    }
}
