using NUnit.Framework.Constraints;
using UnityEngine;

public class GameManager : MonoSingletongeneric<GameManager>
{
    // with this the Game Manager becomes a singleton and any other scripts will have 
    // Access to the Gamemanager.Instance as Isntance is static 
    // As Game manager is inheriting the monosingletongeneric so it becomes hence any script inheriting monosingleton generic will becoem a singleton 

     protected override void Awake()
        {
            base.Awake();
        }
}
