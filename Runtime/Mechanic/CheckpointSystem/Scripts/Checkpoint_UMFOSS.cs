using UnityEngine;
using UnityEngine.Events;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class Checkpoint_UMFOSS : MonoBehaviour, IInteractable_UMFOSS
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private string checkpointID;
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private bool activateOnEnter;

        [Header("Events")]
        public UnityEvent onActivated;
        public UnityEvent onDeactivated;

        [Header("State")]
        [SerializeField] private bool isActive;

        public string CheckpointID => checkpointID;
        public Transform RespawnPoint => respawnPoint != null ? respawnPoint : transform;
        public bool IsActive => isActive;

        private void Awake()
        {
            if (CheckpointManager_UMFOSS.Instance != null)
            {
                CheckpointManager_UMFOSS.Instance.RegisterCheckpoint(this);
            }
        }

        public void Activate(GameObject activator)
        {
            if (isActive) return;

            isActive = true;
            onActivated?.Invoke();
            EventBus.Publish(new OnCheckpointActivated { checkpoint = this, activator = activator });
        }

        public void Deactivate()
        {
            isActive = false;
            onDeactivated?.Invoke();
            EventBus.Publish(new OnCheckpointDeactivated { checkpoint = this });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (activateOnEnter && other.CompareTag("Player"))
            {
                Activate(other.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activateOnEnter && other.CompareTag("Player"))
            {
                Activate(other.gameObject);
            }
        }

        // IInteractable_UMFOSS Implementation
        public void Interact(GameObject interactor) => Activate(interactor);
        public bool CanInteract(GameObject interactor) => !isActive;
        public string GetPromptText() => isActive ? "Checkpoint active" : "Press E to activate";
        public void OnFocused(GameObject interactor) { /* show interact indicator */ }
        public void OnUnfocused(GameObject interactor) { /* hide interact indicator */ }
    }
}
