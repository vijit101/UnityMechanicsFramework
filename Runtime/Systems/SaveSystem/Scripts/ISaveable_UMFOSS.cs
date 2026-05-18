namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Contract for any MonoBehaviour that wants its state saved and restored.
    /// Implement this interface on any script that needs persistence.
    /// Register/deregister in OnEnable/OnDisable for correct lifecycle handling.
    /// </summary>
    public interface ISaveable_UMFOSS
    {
        /// <summary>
        /// Returns a unique identifier for this saveable instance.
        /// Must be unique across ALL saveables in the scene.
        /// Use a stable ID (e.g., a serialized string field) rather than gameObject.name
        /// for dynamically spawned objects.
        /// Examples: "PlayerHealth", "Inventory_Chest_01", "BossHealth_Dragon"
        /// </summary>
        string GetSaveID();

        /// <summary>
        /// Called by SaveSystem when saving. Return a serializable object
        /// representing this script's current state.
        /// Can be a [System.Serializable] struct/class, a primitive, or any
        /// type that JsonUtility can serialize.
        /// The returned object will be serialized to a JSON string by SaveSystem.
        /// </summary>
        object CaptureState();

        /// <summary>
        /// Called by SaveSystem when loading. Receives the JSON string
        /// that was produced by serializing the object from CaptureState.
        /// Cast state to string, then use JsonUtility.FromJson&lt;T&gt;()
        /// to deserialize back to your data type and apply the values.
        /// </summary>
        /// <param name="state">A JSON string representing the saved state. Cast to string before deserializing.</param>
        void RestoreState(object state);
    }
}
