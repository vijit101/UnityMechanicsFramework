using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Attach to the same GameObject that owns a StateMachine_UMFOSS.
    /// Shows current state, duration, and recent history in the Inspector during Play Mode.
    /// </summary>
    public class StateMachineDebugger_UMFOSS : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebug = true;

        [SerializeField, ReadOnly] private string currentStateName;
        [SerializeField, ReadOnly] private float  currentStateDuration;
        [SerializeField, ReadOnly] private string previousStateName;
        [SerializeField, ReadOnly] private List<string> recentHistory = new List<string>();

        private float stateEnteredTime;

        private void OnEnable()
        {
            if (enableDebug)
                EventBus_UMFOSS.Subscribe<StateChangedEvent>(OnStateChanged);
        }

        private void OnDisable()
        {
            EventBus_UMFOSS.Unsubscribe<StateChangedEvent>(OnStateChanged);
        }

        private void OnValidate()
        {
            // react to enableDebug toggle in Inspector during Play Mode
            if (Application.isPlaying)
            {
                if (enableDebug)
                    EventBus_UMFOSS.Subscribe<StateChangedEvent>(OnStateChanged);
                else
                    EventBus_UMFOSS.Unsubscribe<StateChangedEvent>(OnStateChanged);
            }
        }

        private void Update()
        {
            if (!enableDebug) return;
            currentStateDuration = Time.time - stateEnteredTime;
        }

        private void OnStateChanged(StateChangedEvent e)
        {
            if (!enableDebug) return;
            var ownerObject = e.owner as Object;
            if (ownerObject == null || ownerObject != gameObject) return;

            previousStateName = e.previousStateName;
            currentStateName  = e.newStateName;
            stateEnteredTime  = e.timestamp;

            recentHistory.Add($"{e.previousStateName} → {e.newStateName}");
            if (recentHistory.Count > 5)
                recentHistory.RemoveAt(0);
        }

        // draws state name above the GameObject in Scene view — zero cost in builds
        private void OnDrawGizmos()
        {
            if (!enableDebug || string.IsNullOrEmpty(currentStateName)) return;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 1.5f,
                currentStateName
            );
#endif
        }
    }
}
