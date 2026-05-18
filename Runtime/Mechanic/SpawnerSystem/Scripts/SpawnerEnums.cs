namespace GameplayMechanicsUMFOSS.World
{
    public enum SpawnPointMode
    {
        Random,
        Sequential,
        Nearest,
        All
    }

    public enum SpawnShape
    {
        Point,
        Circle,
        Rectangle
    }

    public enum WaveClearCondition
    {
        AllDead,
        TimedEnd,
        Manual
    }
}
