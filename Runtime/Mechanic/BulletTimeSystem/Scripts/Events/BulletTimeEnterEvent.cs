namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>Fired when bullet time begins or switches to a slower active profile.</summary>
    public struct BulletTimeEnterEvent
    {
        public float targetTimeScale;
        public float enterDuration;
    }
}
