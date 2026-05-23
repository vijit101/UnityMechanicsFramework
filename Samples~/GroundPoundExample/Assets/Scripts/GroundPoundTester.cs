using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.GroundPound
{
    /// <summary>
    /// A lightweight sample input bridge that captures player input and forwards 
    /// activation requests to the core GroundPound_UMFOSS system component.
    /// Place this script on your sample Player prefab within the Samples directory.
    /// </summary>
    public class GroundPoundTester : MonoBehaviour
    {
        [Header("System Link")]
        [Tooltip("Reference to the core Ground Pound component attached to this object.")]
        [SerializeField] private Movement.GroundPound_UMFOSS groundPoundSystem;

        private void Awake()
        {
            // Fail-safe: Auto-capture the reference if it wasn't manually dragged into the Inspector slot
            if (groundPoundSystem == null)
            {
                groundPoundSystem = GetComponent<Movement.GroundPound_UMFOSS>();
            }
        }

        private void Update()
        {
            // Defensive check to avoid runtime crash loops if the system link is missing entirely
            if (groundPoundSystem == null || groundPoundSystem.config == null) return;

            // Optional: The main update loop in GroundPound_UMFOSS already monitors the raw input key 
            // from its configuration file. This tester script acts as a secondary verification gateway 
            // or an integration bridge for custom controller mechanics.
            if (Input.GetKeyDown(groundPoundSystem.config.poundKey))
            {
                groundPoundSystem.TriggerGroundPound();
            }
        }
    }
}