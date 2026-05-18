using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// The complete save file structure stored on disk.
    /// Contains metadata (version, timestamp, scene) and all saveable data
    /// as serialized JSON strings keyed by their SaveID.
    /// </summary>
    [System.Serializable]
    public class SaveData_UMFOSS
    {
        /// <summary>
        /// Incremented when the save structure changes.
        /// Used for version migration on load.
        /// </summary>
        public int saveVersion;

        /// <summary>
        /// The slot name this save belongs to (e.g., "Slot1", "AutoSave").
        /// </summary>
        public string saveSlotName;

        /// <summary>
        /// ISO 8601 timestamp of when this save was created.
        /// </summary>
        public string lastSavedTimestamp;

        /// <summary>
        /// The scene that was active when this save was made.
        /// Used to load the correct scene before restoring state.
        /// </summary>
        public string sceneNameOnSave;

        /// <summary>
        /// All saveable data. Key = SaveID, Value = JSON string of the saveable's state.
        /// Uses SerializableDictionary for JsonUtility compatibility.
        /// </summary>
        public SerializableDictionary savedObjects = new SerializableDictionary();
    }

    /// <summary>
    /// Lightweight metadata-only view of a save file.
    /// Used by SaveFileHandler.LoadMetadata to avoid deserializing gameplay payload.
    /// </summary>
    [System.Serializable]
    public class SaveMetadata_UMFOSS
    {
        public int saveVersion;
        public string saveSlotName;
        public string lastSavedTimestamp;
        public string sceneNameOnSave;
    }

    /// <summary>
    /// A Dictionary(string, string) wrapper that works with Unity's JsonUtility.
    /// JsonUtility cannot serialize Dictionary natively, so we use
    /// ISerializationCallbackReceiver to convert to/from parallel lists.
    /// </summary>
    [System.Serializable]
    public class SerializableDictionary : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<string> keys = new List<string>();

        [SerializeField]
        private List<string> values = new List<string>();

        // The actual dictionary used at runtime
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets a value by key.
        /// </summary>
        public string this[string key]
        {
            get { return dictionary[key]; }
            set { dictionary[key] = value; }
        }

        /// <summary>
        /// The number of key-value pairs.
        /// </summary>
        public int Count { get { return dictionary.Count; } }

        /// <summary>
        /// All keys in the dictionary.
        /// </summary>
        public Dictionary<string, string>.KeyCollection Keys
        {
            get { return dictionary.Keys; }
        }

        /// <summary>
        /// Checks if the dictionary contains the specified key.
        /// </summary>
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Tries to get a value by key.
        /// </summary>
        public bool TryGetValue(string key, out string value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Adds or updates a key-value pair.
        /// </summary>
        public void Add(string key, string value)
        {
            dictionary[key] = value;
        }

        /// <summary>
        /// Removes a key-value pair.
        /// </summary>
        public bool Remove(string key)
        {
            return dictionary.Remove(key);
        }

        /// <summary>
        /// Clears all entries.
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
        }

        // Called before JsonUtility serializes this object.
        // Converts the dictionary into parallel key/value lists.
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        // Called after JsonUtility deserializes this object.
        // Rebuilds the dictionary from the parallel lists.
        public void OnAfterDeserialize()
        {
            dictionary = new Dictionary<string, string>();

            int count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                if (!dictionary.ContainsKey(keys[i]))
                {
                    dictionary[keys[i]] = values[i];
                }
                else
                {
                    Debug.LogWarning($"[SaveData] Duplicate key found during deserialization: '{keys[i]}'. Skipping duplicate.");
                }
            }
        }
    }
}
