using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Generic singleton base for <see cref="MonoBehaviour"/> managers.
    /// Duplicate instances are destroyed; first instance persists.
    /// </summary>
    public abstract class MonoSingletonGeneric<T> : MonoBehaviour where T : MonoSingletonGeneric<T>
    {
        private static T _instance;

        public static T Instance => _instance;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
