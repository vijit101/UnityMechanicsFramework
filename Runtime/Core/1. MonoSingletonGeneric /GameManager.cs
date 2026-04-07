using GameplayMechanicsUMFOSS.Core;
using UnityEngine;

/// <summary>
/// Example singleton manager using <see cref="MonoSingletonGeneric{T}"/>.
/// </summary>
public class GameManager : MonoSingletonGeneric<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
    }
}
