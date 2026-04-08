using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Persists <see cref="QuestManager_UMFOSS"/> state and sample world (consumed entities + player position).
    /// </summary>
    public sealed class QuestSampleSaveCoordinator : MonoBehaviour
    {
        public const string PlayerPrefsKey = "UMFOSS_QuestSampleFullSave";

        [SerializeField]
        private Transform player;

        public void SetPlayer(Transform playerTransform)
        {
            player = playerTransform;
        }

        public void Save()
        {
            var m = QuestManager_UMFOSS.Instance;
            if (m == null)
            {
                Debug.LogWarning("QuestSampleSaveCoordinator: no QuestManager.");
                return;
            }

            var questData = (QuestSaveData_UMFOSS)m.CaptureState();
            var reg = QuestSampleWorldRegistry.Instance;
            var consumed = reg != null ? reg.GetConsumedSnapshot() : System.Array.Empty<string>();
            var full = new QuestSampleFullSave
            {
                quest = questData,
                consumedIds = consumed
            };

            if (player != null)
            {
                var p = player.position;
                full.playerX = p.x;
                full.playerY = p.y;
                full.playerZ = p.z;
            }

            var json = JsonUtility.ToJson(full);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
            Debug.Log("Quest sample saved (quest + world + player position).");
        }

        public void Load()
        {
            var m = QuestManager_UMFOSS.Instance;
            if (m == null)
            {
                return;
            }

            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                Debug.Log("No quest sample save found.");
                return;
            }

            var json = PlayerPrefs.GetString(PlayerPrefsKey);
            var full = JsonUtility.FromJson<QuestSampleFullSave>(json);
            if (full == null || full.quest == null)
            {
                Debug.LogWarning("Invalid save data.");
                return;
            }

            m.RestoreState(full.quest);
            if (QuestSampleWorldRegistry.Instance != null)
            {
                QuestSampleWorldRegistry.Instance.LoadConsumedSnapshot(full.consumedIds);
            }

            if (player != null)
            {
                player.position = new Vector3(full.playerX, full.playerY, full.playerZ);
                Physics.SyncTransforms();
                var life = player.GetComponent<QuestSamplePlayerLifecycle>();
                if (life != null)
                {
                    life.SetSpawnPoint(player.position, player.rotation);
                }
            }

            Debug.Log("Quest sample loaded.");
        }
    }
}
