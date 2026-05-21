using UnityEngine;

public class MonoSingletongeneric<T> : MonoBehaviour where T : MonoSingletongeneric<T>
{
    private static T instance;
    public static T Instance { get { return instance; } }
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = (T)this;
        }
        else
        {
            Destroy(this);
        }
    }
}