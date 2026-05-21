namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>Fired when bullet time starts restoring normal speed.</summary>
    public struct BulletTimeExitEvent
    {
        public float restoredTimeScale;
        public float exitDuration;
    }
}
