using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Central Save System coordinator.
    /// Manages registration of ISaveable components, triggers save/load operations,
    /// handles version migration, auto-save, and publishes events.
    /// Inherits MonoSingletongeneric to persist across scene loads.
    /// </summary>
    public class SaveSystem_UMFOSS : MonoSingletongeneric<SaveSystem_UMFOSS>
    {
        // ─────────────────────────────────────────────
        // Serialized Fields
        // ─────────────────────────────────────────────

        [Header("Save Settings")]
        [Tooltip("Current save format version. Increment when the save structure changes.")]
        [SerializeField] private int currentSaveVersion = 1;

        [Tooltip("Automatically save when a scene is about to unload.")]
        [SerializeField] private bool autoSaveOnSceneUnload = false;

        [Tooltip("Auto-save interval in seconds. Set to 0 to disable.")]
        [SerializeField] private float autoSaveInterval = 0f;

        [Tooltip("Default slot name used when no slot is specified.")]
        [SerializeField] private string defaultSlotName = "Slot1";

        [Header("Security")]
        [Tooltip("Enable XOR encryption on save files. Anti-tamper, not cryptographic.")]
        [SerializeField] private bool encryptSaveFile = false;

        [Tooltip("XOR encryption key. Set in Inspector, never hardcode.")]
        [SerializeField] private string encryptionKey = "";

        // ─────────────────────────────────────────────
        // Private Fields
        // ─────────────────────────────────────────────

        /// <summary>
        /// All currently registered saveables. Key = SaveID, Value = ISaveable reference.
        /// </summary>
        private Dictionary<string, ISaveable_UMFOSS> registeredSaveables = new Dictionary<string, ISaveable_UMFOSS>();

        /// <summary>
        /// Cached save data from the most recent load. Used to restore state
        /// after a scene transition completes.
        /// </summary>
        private SaveData_UMFOSS pendingLoadData = null;

        /// <summary>
        /// The slot name for the pending load operation.
        /// </summary>
        private string pendingLoadSlot = "";

        /// <summary>
        /// Coroutine reference for auto-save, so we can stop it.
        /// </summary>
        private Coroutine autoSaveCoroutine = null;

        // ─────────────────────────────────────────────
        // Events
        // ─────────────────────────────────────────────

        /// <summary>Fired after a successful save.</summary>
        public event SaveSystemEvents_UMFOSS.SaveEvent OnGameSaved;

        /// <summary>Fired after a successful load and state restoration.</summary>
        public event SaveSystemEvents_UMFOSS.LoadEvent OnGameLoaded;

        /// <summary>Fired after a save file is deleted.</summary>
        public event SaveSystemEvents_UMFOSS.DeleteEvent OnSaveDeleted;

        /// <summary>Fired when a save operation fails.</summary>
        public event SaveSystemEvents_UMFOSS.FailEvent OnSaveFailed;

        /// <summary>Fired when a load operation fails.</summary>
        public event SaveSystemEvents_UMFOSS.FailEvent OnLoadFailed;

        /// <summary>Fired when a new game is started on a slot.</summary>
        public event SaveSystemEvents_UMFOSS.NewGameEvent OnNewGameStarted;

        // ─────────────────────────────────────────────
        // Unity Lifecycle
        // ─────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            // Subscribe to scene unload for auto-save
            if (autoSaveOnSceneUnload)
            {
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }

            // Start auto-save coroutine if interval is set
            StartAutoSave();
        }

        private void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            StopAutoSave();
        }

        // ─────────────────────────────────────────────
        // Registration API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Registers an ISaveable so its state will be included in save operations.
        /// Call this in OnEnable of the implementing script.
        /// Includes null-safety: does nothing if SaveSystem is not initialized.
        /// </summary>
        /// <param name="saveable">The saveable to register.</param>
        public void Register(ISaveable_UMFOSS saveable)
        {
            if (saveable == null)
            {
                Debug.LogWarning("[SaveSystem] Attempted to register a null saveable. Ignoring.");
                return;
            }

            string id = saveable.GetSaveID();

            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[SaveSystem] Saveable returned null or empty SaveID. Registration rejected.");
                return;
            }

            if (registeredSaveables.ContainsKey(id))
            {
                Debug.LogWarning($"[SaveSystem] Saveable with ID '{id}' is already registered. Overwriting with new reference.");
            }

            registeredSaveables[id] = saveable;
            Debug.Log($"[SaveSystem] Registered saveable: '{id}' (Total: {registeredSaveables.Count})");

            // If we have pending load data (scene just loaded), restore this saveable immediately
            if (pendingLoadData != null)
            {
                RestoreSingleSaveable(saveable, pendingLoadData);
            }
        }

        /// <summary>
        /// Deregisters an ISaveable so it won't be included in future save operations.
        /// Call this in OnDisable of the implementing script.
        /// </summary>
        /// <param name="saveable">The saveable to deregister.</param>
        public void Deregister(ISaveable_UMFOSS saveable)
        {
            if (saveable == null) return;

            string id = saveable.GetSaveID();

            if (string.IsNullOrEmpty(id)) return;

            if (registeredSaveables.Remove(id))
            {
                Debug.Log($"[SaveSystem] Deregistered saveable: '{id}' (Remaining: {registeredSaveables.Count})");
            }
        }

        // ─────────────────────────────────────────────
        // Core API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Saves the current state of all registered saveables to the specified slot.
        /// </summary>
        /// <param name="slotName">The slot to save to. Defaults to the configured default slot.</param>
        public void Save(string slotName = null)
        {
            if (string.IsNullOrEmpty(slotName)) slotName = defaultSlotName;

            Debug.Log($"[SaveSystem] === SAVE START === Slot: '{slotName}', Saveables: {registeredSaveables.Count}");

            // Create save data
            SaveData_UMFOSS saveData = new SaveData_UMFOSS
            {
                saveVersion = currentSaveVersion,
                saveSlotName = slotName,
                lastSavedTimestamp = System.DateTime.Now.ToString("o"), // ISO 8601
                sceneNameOnSave = SceneManager.GetActiveScene().name,
                savedObjects = new SerializableDictionary()
            };

            // Capture state from all registered saveables
            foreach (var kvp in registeredSaveables)
            {
                string saveID = kvp.Key;
                ISaveable_UMFOSS saveable = kvp.Value;

                try
                {
                    object state = saveable.CaptureState();

                    if (state == null)
                    {
                        Debug.LogWarning($"[SaveSystem] Saveable '{saveID}' returned null state. Skipping.");
                        continue;
                    }

                    // Serialize the state object to JSON string
                    string json = JsonUtility.ToJson(state);
                    saveData.savedObjects.Add(saveID, json);

                    Debug.Log($"[SaveSystem] Captured: '{saveID}' -> {json.Length} chars");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SaveSystem] Failed to capture state from '{saveID}': {ex.Message}");
                }
            }

            // Write to disk
            bool success = SaveFileHandler_UMFOSS.Save(saveData, slotName, encryptSaveFile, encryptionKey);

            if (success)
            {
                Debug.Log($"[SaveSystem] === SAVE COMPLETE === Slot: '{slotName}', Path: {SaveFileHandler_UMFOSS.GetFilePath(slotName)}");
                OnGameSaved?.Invoke(slotName);
            }
            else
            {
                string reason = string.IsNullOrEmpty(SaveFileHandler_UMFOSS.LastErrorMessage)
                    ? "Failed to write save file to disk."
                    : SaveFileHandler_UMFOSS.LastErrorMessage;
                Debug.LogError($"[SaveSystem] === SAVE FAILED === Slot: '{slotName}', Reason: {reason}");
                OnSaveFailed?.Invoke(slotName, reason);
            }
        }

        /// <summary>
        /// Loads a save file and restores state to all registered saveables.
        /// If the save file references a different scene, that scene is loaded first.
        /// </summary>
        /// <param name="slotName">The slot to load from.</param>
        public void Load(string slotName = null)
        {
            if (string.IsNullOrEmpty(slotName)) slotName = defaultSlotName;

            Debug.Log($"[SaveSystem] === LOAD START === Slot: '{slotName}'");

            // Load from disk
            SaveData_UMFOSS saveData = SaveFileHandler_UMFOSS.Load(slotName, encryptSaveFile, encryptionKey);

            if (saveData == null)
            {
                string reason = string.IsNullOrEmpty(SaveFileHandler_UMFOSS.LastErrorMessage)
                    ? "Save file not found, empty, or corrupt."
                    : SaveFileHandler_UMFOSS.LastErrorMessage;
                Debug.LogError($"[SaveSystem] === LOAD FAILED === Slot: '{slotName}', Reason: {reason}");
                OnLoadFailed?.Invoke(slotName, reason);
                return;
            }

            // Version check
            if (saveData.saveVersion > currentSaveVersion)
            {
                string reason = $"Save version ({saveData.saveVersion}) is newer than current version ({currentSaveVersion}). Cannot load a save from a newer build.";
                Debug.LogError($"[SaveSystem] === LOAD BLOCKED === {reason}");
                OnLoadFailed?.Invoke(slotName, reason);
                return;
            }

            // Version migration
            if (saveData.saveVersion < currentSaveVersion)
            {
                Debug.Log($"[SaveSystem] Migrating save from version {saveData.saveVersion} to {currentSaveVersion}...");
                MigrateSave(saveData);
                saveData.saveVersion = currentSaveVersion;
            }

            // Check if we need to load a different scene
            string currentScene = SceneManager.GetActiveScene().name;
            string savedScene = saveData.sceneNameOnSave;

            if (!string.IsNullOrEmpty(savedScene) && savedScene != currentScene)
            {
                Debug.Log($"[SaveSystem] Scene mismatch: current='{currentScene}', saved='{savedScene}'. Loading saved scene...");
                pendingLoadData = saveData;
                pendingLoadSlot = slotName;
                StartCoroutine(LoadSceneAndRestoreState(savedScene, slotName));
            }
            else
            {
                // Same scene — restore immediately
                RestoreAllSaveables(saveData);
                pendingLoadData = null;
                pendingLoadSlot = string.Empty;
                Debug.Log($"[SaveSystem] === LOAD COMPLETE === Slot: '{slotName}'");
                OnGameLoaded?.Invoke(slotName);
            }
        }

        /// <summary>
        /// Deletes a save file from disk.
        /// </summary>
        /// <param name="slotName">The slot to delete.</param>
        public void Delete(string slotName)
        {
            if (string.IsNullOrEmpty(slotName)) slotName = defaultSlotName;

            Debug.Log($"[SaveSystem] Deleting save: '{slotName}'");

            bool success = SaveFileHandler_UMFOSS.Delete(slotName);

            if (success)
            {
                OnSaveDeleted?.Invoke(slotName);
            }
        }

        /// <summary>
        /// Starts a new game by deleting the save file for the slot
        /// and letting all saveables remain at their default/current state.
        /// </summary>
        /// <param name="slotName">The slot to reset.</param>
        public void NewGame(string slotName = null)
        {
            if (string.IsNullOrEmpty(slotName)) slotName = defaultSlotName;

            Debug.Log($"[SaveSystem] === NEW GAME === Slot: '{slotName}'");

            // Delete existing save
            SaveFileHandler_UMFOSS.Delete(slotName);

            // Clear pending load data
            pendingLoadData = null;
            pendingLoadSlot = "";

            OnNewGameStarted?.Invoke(slotName);
        }

        // ─────────────────────────────────────────────
        // Slot Management API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Checks if a save file exists for the given slot.
        /// </summary>
        public bool SaveExists(string slotName)
        {
            return SaveFileHandler_UMFOSS.SaveExists(slotName);
        }

        /// <summary>
        /// Returns all slot names that have save files on disk.
        /// </summary>
        public List<string> GetAllSaveSlots()
        {
            return SaveFileHandler_UMFOSS.GetAllSaveSlots();
        }

        /// <summary>
        /// Reads just the metadata of a save file (timestamp, scene, version).
        /// Does not process individual saveable data — fast for UI display.
        /// </summary>
        public SaveData_UMFOSS GetSaveMetadata(string slotName)
        {
            return SaveFileHandler_UMFOSS.LoadMetadata(slotName, encryptSaveFile, encryptionKey);
        }

        /// <summary>
        /// Returns the full file path for the given slot.
        /// Useful for displaying in debug UI.
        /// </summary>
        public string GetSaveFilePath(string slotName = null)
        {
            if (string.IsNullOrEmpty(slotName)) slotName = defaultSlotName;
            return SaveFileHandler_UMFOSS.GetFilePath(slotName);
        }

        // ─────────────────────────────────────────────
        // Version Migration
        // ─────────────────────────────────────────────

        /// <summary>
        /// Handles version migration for save files.
        /// Override or extend this method to add custom migration logic.
        /// Each case in the switch falls through to the next, applying
        /// cumulative upgrades from the old version to current.
        /// </summary>
        /// <param name="saveData">The save data to migrate in-place.</param>
        private void MigrateSave(SaveData_UMFOSS saveData)
        {
            int fromVersion = saveData.saveVersion;

            if (saveData.savedObjects == null)
            {
                saveData.savedObjects = new SerializableDictionary();
            }

            switch (fromVersion)
            {
                case 0:
                    // Migration v0 -> v1
                    // Populate new metadata fields and map known legacy IDs.
                    if (string.IsNullOrEmpty(saveData.saveSlotName)) saveData.saveSlotName = defaultSlotName;
                    if (string.IsNullOrEmpty(saveData.lastSavedTimestamp)) saveData.lastSavedTimestamp = System.DateTime.Now.ToString("o");
                    if (string.IsNullOrEmpty(saveData.sceneNameOnSave)) saveData.sceneNameOnSave = SceneManager.GetActiveScene().name;

                    const string legacyHealthID = "HealthSystem";
                    const string currentHealthID = "HealthSystem_Player";
                    string legacyHealthJson;
                    if (saveData.savedObjects.TryGetValue(legacyHealthID, out legacyHealthJson) && !saveData.savedObjects.ContainsKey(currentHealthID))
                    {
                        saveData.savedObjects.Add(currentHealthID, legacyHealthJson);
                        saveData.savedObjects.Remove(legacyHealthID);
                    }

                    Debug.Log("[SaveSystem] Migrated save from v0 → v1");
                    break;

                case 1:
                    // Current version. Keep for future migrations.
                    break;

                default:
                    Debug.LogWarning($"[SaveSystem] No migration path from version {fromVersion}. Data may be incomplete.");
                    break;
            }
        }

        // ─────────────────────────────────────────────
        // Private Methods
        // ─────────────────────────────────────────────

        /// <summary>
        /// Restores state for all registered saveables from the given save data.
        /// Missing IDs are skipped gracefully with a warning.
        /// </summary>
        private void RestoreAllSaveables(SaveData_UMFOSS saveData)
        {
            int restored = 0;
            int skipped = 0;

            foreach (var kvp in registeredSaveables)
            {
                string saveID = kvp.Key;
                ISaveable_UMFOSS saveable = kvp.Value;

                string stateJson;
                if (saveData.savedObjects.TryGetValue(saveID, out stateJson))
                {
                    try
                    {
                        saveable.RestoreState(stateJson);
                        Debug.Log($"[SaveSystem] Restored: '{saveID}'");
                        restored++;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[SaveSystem] Failed to restore '{saveID}': {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SaveSystem] No saved data found for '{saveID}'. Leaving at default state. (New mechanic added after this save was made?)");
                    skipped++;
                }
            }

            // Log any SaveIDs in the file that have no registered saveable
            foreach (string savedKey in saveData.savedObjects.Keys)
            {
                if (!registeredSaveables.ContainsKey(savedKey))
                {
                    Debug.LogWarning($"[SaveSystem] Save file contains data for '{savedKey}' but no saveable is registered for it. (Mechanic removed or not loaded?)");
                }
            }

            Debug.Log($"[SaveSystem] Restore complete: {restored} restored, {skipped} skipped.");
        }

        /// <summary>
        /// Restores a single saveable from pending load data.
        /// Used when a saveable registers after a scene load.
        /// </summary>
        private void RestoreSingleSaveable(ISaveable_UMFOSS saveable, SaveData_UMFOSS saveData)
        {
            string saveID = saveable.GetSaveID();

            string stateJson;
            if (saveData.savedObjects.TryGetValue(saveID, out stateJson))
            {
                try
                {
                    saveable.RestoreState(stateJson);
                    Debug.Log($"[SaveSystem] Late-restored: '{saveID}'");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SaveSystem] Failed to late-restore '{saveID}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads a scene asynchronously, then restores saveable state.
        /// </summary>
        private IEnumerator LoadSceneAndRestoreState(string sceneName, string slotName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            if (asyncLoad == null)
            {
                string reason = $"Failed to start async load for scene '{sceneName}'.";
                Debug.LogError($"[SaveSystem] {reason}");
                OnLoadFailed?.Invoke(slotName, reason);
                pendingLoadData = null;
                yield break;
            }

            // Wait for scene to finish loading
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Wait one more frame for all OnEnable calls to fire (saveables register)
            yield return null;

            // Restore state
            if (pendingLoadData != null)
            {
                RestoreAllSaveables(pendingLoadData);
                Debug.Log($"[SaveSystem] === LOAD COMPLETE (after scene load) === Slot: '{slotName}'");
                OnGameLoaded?.Invoke(slotName);
                pendingLoadData = null;
                pendingLoadSlot = string.Empty;
            }
        }

        /// <summary>
        /// Called when a scene is unloaded. Triggers auto-save if enabled.
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            if (autoSaveOnSceneUnload)
            {
                Debug.Log($"[SaveSystem] Auto-saving on scene unload: '{scene.name}'");
                Save("AutoSave");
            }
        }

        // ─────────────────────────────────────────────
        // Auto-Save
        // ─────────────────────────────────────────────

        /// <summary>
        /// Starts the auto-save coroutine if interval > 0.
        /// </summary>
        private void StartAutoSave()
        {
            if (autoSaveInterval > 0f)
            {
                StopAutoSave();
                autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
                Debug.Log($"[SaveSystem] Auto-save enabled: every {autoSaveInterval} seconds.");
            }
        }

        /// <summary>
        /// Stops the auto-save coroutine.
        /// </summary>
        private void StopAutoSave()
        {
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
                autoSaveCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine that triggers a save at regular intervals.
        /// </summary>
        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSaveInterval);
                Debug.Log("[SaveSystem] Auto-save triggered.");
                Save("AutoSave");
            }
        }
    }
}
