using System;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    // These are stub definitions to ensure CheckpointSystem scripts compile.
    // If you already have these in your project, you can delete this file.

    public interface IInteractable_UMFOSS
    {
        void Interact(GameObject interactor);
        bool CanInteract(GameObject interactor);
        string GetPromptText();
        void OnFocused(GameObject interactor);
        void OnUnfocused(GameObject interactor);
    }

    public class HealthSystem_UMFOSS : MonoBehaviour
    {
        public void Heal(float amount) { }
        public float GetMaxHealth() { return 100f; }
        public void TakeDamage(float amount) 
        { 
            EventBus.Publish(new DeathEvent { source = gameObject }); 
        }
    }

    public class InventorySystem_UMFOSS : MonoBehaviour
    {
        public void ClearInventory() { }
    }

    public struct DeathEvent
    {
        public GameObject source;
    }
}
