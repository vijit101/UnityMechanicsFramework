using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.UI;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Editor.CheckpointSystem
{
    /// <summary>
    /// Editor tool — Unity menu: Tools > UMFOSS > Build Checkpoint Demo Scene
    /// Wipes the current scene and builds the full 3-platform checkpoint demo
    /// with all GameObjects, components, and Inspector connections pre-wired.
    /// No manual setup required after running this.
    /// </summary>
    public static class CheckpointSceneBuilder
    {
        [MenuItem("Tools/UMFOSS/Build Checkpoint Demo Scene")]
        public static void BuildScene()
        {
            // Ask before wiping the current scene
            if (!EditorUtility.DisplayDialog(
                "Build Checkpoint Demo Scene",
                "This will REPLACE the current scene with the Checkpoint demo layout.\n\nContinue?",
                "Yes, build it",
                "Cancel"))
                return;

            // Open the DemoScene (creates a clean starting state)
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // ─────────────────────────────────────────────────────────────────
            // ENVIRONMENT
            // ─────────────────────────────────────────────────────────────────

            var floor = CreateCube("Floor",      new Vector3(0, 0, 0),     new Vector3(30, 1, 10));
            CreateCube("Platform_1", new Vector3(-8, 1.5f, 0), new Vector3(6, 1, 6));
            CreateCube("Platform_2", new Vector3(0, 3f, 0),    new Vector3(6, 1, 6));
            CreateCube("Platform_3", new Vector3(8, 4.5f, 0),  new Vector3(6, 1, 6));

            // ─────────────────────────────────────────────────────────────────
            // CHECKPOINTS
            // ─────────────────────────────────────────────────────────────────

            var cpA = CreateCheckpoint("Checkpoint_A", new Vector3(-8, 3f, 0),   "Checkpoint_A", autoTrigger: true);
            var cpB = CreateCheckpoint("Checkpoint_B", new Vector3(0, 5f, 0),    "Checkpoint_B", autoTrigger: false);
            var cpC = CreateCheckpoint("Checkpoint_C", new Vector3(8, 6.5f, 0),  "Checkpoint_C", autoTrigger: true);

            // We resolve via AppDomain to avoid assembly-name mismatches if you dragged files manually
            var allTypes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());

            // Make the floor lethal!
            var dzType = allTypes.FirstOrDefault(t => t.Name == "CheckpointDemoDeadzone");
            if (dzType != null) floor.AddComponent(dzType);
            else Debug.LogWarning("[SceneBuilder] CheckpointDemoDeadzone script not found.");

            // ─────────────────────────────────────────────────────────────────
            // PLAYER
            // ─────────────────────────────────────────────────────────────────

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(-8, 3.5f, 0);
            player.tag = "Player";

            // Remove 3D collider, add 2D physics
            UnityEngine.Object.DestroyImmediate(player.GetComponent<Collider>());
            var col2D = player.AddComponent<CapsuleCollider2D>();
            col2D.size = new Vector2(1, 2);

            var rb = player.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            player.AddComponent<HealthSystem_UMFOSS>();

            // We resolve via AppDomain to avoid assembly-name mismatches if you dragged files manually
            var testerType = allTypes.FirstOrDefault(t => t.Name == "CheckpointDemoTester");
            if (testerType != null) player.AddComponent(testerType);
            else Debug.LogWarning("[SceneBuilder] CheckpointDemoTester script not found in project.");

            var playerMoveType = allTypes.FirstOrDefault(t => t.Name == "CheckpointDemoPlayer");
            if (playerMoveType != null) player.AddComponent(playerMoveType);
            else Debug.LogWarning("[SceneBuilder] CheckpointDemoPlayer script not found in project.");

            // Color the player blue so it's easy to spot
            var playerMat = new Material(Shader.Find("Standard"));
            playerMat.color = new Color(0.2f, 0.5f, 1f);
            player.GetComponent<Renderer>().sharedMaterial = playerMat;

            // ─────────────────────────────────────────────────────────────────
            // CHECKPOINT MANAGER
            // ─────────────────────────────────────────────────────────────────

            var managerGO = new GameObject("Manager_Checkpoint");
            var manager   = managerGO.AddComponent<CheckpointManager_UMFOSS>();

            // Wire starting checkpoint via SerializedObject so Unity tracks the reference
            var managerSO = new SerializedObject(manager);
            managerSO.FindProperty("startingCheckpoint").objectReferenceValue = cpA;
            managerSO.FindProperty("respawnDelay").floatValue = 0f;
            managerSO.FindProperty("respawnHealthPercent").floatValue = 1f;
            managerSO.FindProperty("keepInventoryOnDeath").boolValue  = true;
            managerSO.ApplyModifiedProperties();

            // ─────────────────────────────────────────────────────────────────
            // HUD CANVAS
            // ─────────────────────────────────────────────────────────────────

            var canvas = new GameObject("Canvas");
            var canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            var hudTextGO  = new GameObject("HUD_Text");
            hudTextGO.transform.SetParent(canvas.transform, false);
            var rect = hudTextGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot     = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(420, 200);

            var text       = hudTextGO.AddComponent<Text>();
            text.text      = "Checkpoint HUD — press Play";
            text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize  = 15;
            text.color     = Color.white;

            var hudManagerGO = new GameObject("HUDManager");
            var hudType = allTypes.FirstOrDefault(t => t.Name == "CheckpointHUD");
            if (hudType != null)
            {
                var hud   = hudManagerGO.AddComponent(hudType) as UnityEngine.MonoBehaviour;
                var hudSO = new SerializedObject(hud);
                hudSO.FindProperty("hudText").objectReferenceValue = text;
                hudSO.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[SceneBuilder] CheckpointHUD not found. " +
                    "Import the CheckpointSystem sample first, then re-run the builder.");
            }

            // EventSystem (required for UI)
            if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // ─────────────────────────────────────────────────────────────────
            // CAMERA — reposition to see the whole layout
            // ─────────────────────────────────────────────────────────────────

            var cam = Camera.main;
            if (cam == null) 
            {
                var camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
                camGO.tag = "MainCamera";
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            cam.orthographic = true; // 2D flat mode
            cam.orthographicSize = 6f; // Zoom level
            cam.transform.position = new Vector3(0, 4, -20);
            cam.transform.rotation = Quaternion.identity; // Look straight ahead

            // ─────────────────────────────────────────────────────────────────
            // SAVE
            // ─────────────────────────────────────────────────────────────────

            EditorSceneManager.SaveScene(
                EditorSceneManager.GetActiveScene(),
                "Assets/Samples/CheckpointSystem/Assets/Scenes/DemoScene.unity");

            Debug.Log("[CheckpointSceneBuilder] Demo scene built and saved. Press Play!");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private static GameObject CreateCube(string name, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name                  = name;
            go.transform.position    = pos;
            go.transform.localScale  = scale;

            // Remove 3D collider and use 2D ground
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.AddComponent<BoxCollider2D>();

            return go;
        }

        private static Checkpoint_UMFOSS CreateCheckpoint(
            string name, Vector3 pos, string id, bool autoTrigger)
        {
            var go = new GameObject(name);
            go.transform.position = pos;

            // Visual pole
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0, 0.5f, 0);
            visual.transform.localScale    = new Vector3(0.3f, 1f, 0.3f);

            // Remove the default solid collider from the cylinder so the player doesn't get stuck in it
            var solidCol = visual.GetComponent<Collider>();
            if (solidCol != null) UnityEngine.Object.DestroyImmediate(solidCol);

            // Color: green for auto-trigger, yellow for interact
            var mat   = new Material(Shader.Find("Standard"));
            mat.color = autoTrigger ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.85f, 0.1f);
            visual.GetComponent<Renderer>().sharedMaterial = mat;

            // Trigger collider (only on auto-trigger checkpoints)
            if (autoTrigger)
            {
                var col          = go.AddComponent<BoxCollider2D>();
                col.isTrigger    = true;
                col.size         = new Vector2(2, 3);
                col.offset       = new Vector2(0, 1);
            }

            // Respawn marker child
            var respawnPoint              = new GameObject("RespawnPoint");
            respawnPoint.transform.SetParent(go.transform, false);
            respawnPoint.transform.localPosition = new Vector3(0, 2f, 0);

            // Script
            var cp = go.AddComponent<Checkpoint_UMFOSS>();
            var so = new SerializedObject(cp);
            so.FindProperty("checkpointID").stringValue              = id;
            so.FindProperty("activateOnEnter").boolValue             = autoTrigger;
            so.FindProperty("respawnPoint").objectReferenceValue     = respawnPoint.transform;
            so.ApplyModifiedProperties();

            // Hook up UnityEvents to make the visual disappear when activated, and reappear if reset
            UnityEventTools.AddBoolPersistentListener(cp.onActivated,   new UnityEngine.Events.UnityAction<bool>(visual.SetActive), false);
            UnityEventTools.AddBoolPersistentListener(cp.onDeactivated, new UnityEngine.Events.UnityAction<bool>(visual.SetActive), true);

            return cp;
        }
    }
}
