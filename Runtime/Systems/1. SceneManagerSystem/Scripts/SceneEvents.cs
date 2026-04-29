namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>Fired the moment a scene load sequence begins.</summary>
    public struct SceneLoadStartEvent
    {
        public string fromScene;
        public string toScene;
    }

    /// <summary>Fired every frame during async load with normalized 0-1 progress.</summary>
    public struct SceneLoadProgressEvent
    {
        public float progress;
    }

    /// <summary>Fired when the new scene is fully active and faded in.</summary>
    public struct SceneLoadCompleteEvent
    {
        public string sceneName;
    }

    /// <summary>Fired after an additive scene is pushed onto the stack.</summary>
    public struct ScenePushedEvent
    {
        public string sceneName;
    }

    /// <summary>Fired after the top scene of the stack has been popped.</summary>
    public struct ScenePoppedEvent
    {
        public string sceneName;
    }

    /// <summary>Fired after the current scene has finished reloading.</summary>
    public struct SceneReloadedEvent
    {
        public string sceneName;
    }

    /// <summary>Used internally at transition start/end to lock or unlock player input.</summary>
    public struct InputLockEvent
    {
        public bool locked;
    }
}
