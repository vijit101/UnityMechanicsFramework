using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Marks the GameObject as belonging to the persistent scene.
    /// SceneManager_UMFOSS will never unload a scene that this component lives in.
    /// Place on a single GameObject in the bootstrap scene that hosts your singletons.
    /// </summary>
    public class PersistentScene_UMFOSS : MonoBehaviour
    {
        private void Awake()
        {
            // Survive scene unloads even outside the SceneManager flow.
            DontDestroyOnLoad(gameObject);
        }
    }
}
