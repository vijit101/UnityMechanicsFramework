using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Builds camera, UI, quest runtime, and a small explorable world for the quest sample (single root object in scene).
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class QuestSampleSceneBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var tracker = CreateText(canvasGo.transform, "QuestTracker", 24, TextAnchor.UpperLeft, new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(20, -20), new Vector2(920, 580));
            var log = CreateText(canvasGo.transform, "EventLog", 20, TextAnchor.UpperRight, new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(-20, -20), new Vector2(780, 380));
            var rewards = CreateText(canvasGo.transform, "Rewards", 22, TextAnchor.LowerRight, new Vector2(1, 0),
                new Vector2(1, 0), new Vector2(-20, 20), new Vector2(520, 140));
            var controls = CreateText(canvasGo.transform, "Controls", 18, TextAnchor.LowerLeft, new Vector2(0, 0),
                new Vector2(0, 0), new Vector2(20, 20), new Vector2(1280, 140));
            var retryHint = CreateText(canvasGo.transform, "RetryHint", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -95f), new Vector2(920f, 52f));
            retryHint.color = new Color(1f, 0.92f, 0.35f);
            retryHint.gameObject.SetActive(false);

            CreateDeathPanel(canvasGo.transform, out var deathRoot, out var deathTitle, out var deathCountdown,
                out var deathRespawnBtn);

            var questGo = new GameObject("QuestRuntime");
            questGo.AddComponent<QuestSampleWorldRegistry>();
            questGo.AddComponent<QuestSampleSaveCoordinator>();
            questGo.AddComponent<QuestManager_UMFOSS>();
            questGo.AddComponent<QuestSystem_UMFOSS>();
            questGo.AddComponent<QuestSampleRuntimeSetup>();
            questGo.AddComponent<QuestSampleClearCampEncounterHooks>();
            questGo.AddComponent<QuestSampleInput>();
            var hud = questGo.AddComponent<QuestSampleHud>();
            hud.Configure(tracker, log, controls, rewards);
            hud.ConfigureRetryHint(retryHint);

            var player = BuildWorld();
            var life = player.GetComponent<QuestSamplePlayerLifecycle>();
            hud.ConfigureDeath(deathRoot, deathTitle, deathCountdown, deathRespawnBtn, life);
            var save = questGo.GetComponent<QuestSampleSaveCoordinator>();
            save.SetPlayer(player.transform);
        }

        private static Transform BuildWorld()
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.89f);
            light.intensity = 1.05f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
            SetColor(ground, new Color(0.22f, 0.32f, 0.22f));

            CreateBoundaryWalls(20f, 3f, 1f);

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            SetColor(player, new Color(0.25f, 0.45f, 0.85f));
            Object.Destroy(player.GetComponent<CapsuleCollider>());
            var cc = player.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 0f, 0f);
            cc.height = 2f;
            cc.radius = 0.45f;
            player.AddComponent<QuestSamplePlayerMotor>();
            player.AddComponent<QuestSamplePlayerCombat>();
            player.AddComponent<QuestSamplePlayerLifecycle>();

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.13f, 0.16f);
            camGo.AddComponent<AudioListener>();
            camGo.transform.SetParent(player.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 3.2f, -6.5f);
            camGo.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);

            CreateLabelCube("Label_Spawn", new Vector3(0f, 0.05f, -2.5f), new Vector3(3f, 0.1f, 1f), new Color(0.3f, 0.3f, 0.35f));

            var questBoard = CreateNpcCube("QuestBoard", new Vector3(-4f, 1f, -3.5f), new Color(0.35f, 0.55f, 0.92f));
            var questBoardTrigger = questBoard.AddComponent<BoxCollider>();
            questBoardTrigger.isTrigger = true;
            questBoardTrigger.size = new Vector3(5f, 3f, 5f);
            questBoard.AddComponent<QuestSampleQuestStartPoint>();
            CreateLabelCube("Label_QuestBoard", new Vector3(-4f, 0.05f, -5.5f), new Vector3(5f, 0.1f, 1f), new Color(0.35f, 0.35f, 0.4f));

            var merchant = CreateNpcCube("Merchant", new Vector3(10f, 1f, 6f), new Color(0.85f, 0.65f, 0.2f));
            var merchantTrigger = merchant.AddComponent<BoxCollider>();
            merchantTrigger.isTrigger = true;
            merchantTrigger.size = new Vector3(4f, 3f, 4f);
            merchantTrigger.center = new Vector3(0f, 0f, 0f);
            merchant.AddComponent<QuestSampleMerchant>();

            CreateLabelCube("Label_Merchant", new Vector3(10f, 0.05f, 8.5f), new Vector3(4f, 0.1f, 1f), new Color(0.35f, 0.35f, 0.4f));

            CreateGoblin("Goblin_A", new Vector3(-9f, 1f, -8f));
            CreateGoblin("Goblin_B", new Vector3(-11f, 1f, -10f));
            CreateGoblin("Goblin_C", new Vector3(-7f, 1f, -11f));

            CreatePickup("Loot_Goblin", new Vector3(-9f, 0.6f, -6f), "GoblinLoot", new Color(0.6f, 0.2f, 0.7f));
            CreatePickup("Ore_A", new Vector3(14f, 0.6f, 2f), "IronOre", new Color(0.45f, 0.45f, 0.5f));
            CreatePickup("Ore_B", new Vector3(15f, 0.6f, -1f), "IronOre", new Color(0.45f, 0.45f, 0.5f));
            CreatePickup("Bonus_Trinket", new Vector3(-4f, 0.6f, 10f), "BonusTrinket", new Color(0.2f, 0.85f, 0.75f));

            CreateZone("Zone_EastGate", new Vector3(22f, 2f, 0f), new Vector3(8f, 4f, 8f), "EastGate");
            CreateZone("Zone_NorthTower", new Vector3(0f, 2f, 22f), new Vector3(8f, 4f, 8f), "NorthTower");

            CreateLabelCube("Label_Camp", new Vector3(-9f, 0.05f, -12f), new Vector3(8f, 0.1f, 1f), new Color(0.35f, 0.35f, 0.4f));
            CreateLabelCube("Label_East", new Vector3(22f, 0.05f, -4f), new Vector3(6f, 0.1f, 1f), new Color(0.35f, 0.35f, 0.4f));
            CreateLabelCube("Label_North", new Vector3(-4f, 0.05f, 22f), new Vector3(6f, 0.1f, 1f), new Color(0.35f, 0.35f, 0.4f));

            return player.transform;
        }

        private static void CreateBoundaryWalls(float halfExtent, float wallHeight, float thickness)
        {
            var y = wallHeight * 0.5f;
            var span = halfExtent * 2f + thickness * 2f;
            var zPos = halfExtent + thickness * 0.5f;
            var xPos = halfExtent + thickness * 0.5f;
            CreateWall("Wall_North", new Vector3(0f, y, zPos), new Vector3(span, wallHeight, thickness));
            CreateWall("Wall_South", new Vector3(0f, y, -zPos), new Vector3(span, wallHeight, thickness));
            CreateWall("Wall_East", new Vector3(xPos, y, 0f), new Vector3(thickness, wallHeight, span));
            CreateWall("Wall_West", new Vector3(-xPos, y, 0f), new Vector3(thickness, wallHeight, span));
        }

        private static GameObject CreateWall(string name, Vector3 worldCenter, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = worldCenter;
            go.transform.localScale = scale;
            SetColor(go, new Color(0.38f, 0.34f, 0.3f));
            return go;
        }

        private static void CreateDeathPanel(Transform canvasParent, out GameObject root, out Text titleText,
            out Text countdownText, out Button respawnButton)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            root = new GameObject("DeathOverlay");
            root.transform.SetParent(canvasParent, false);
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            var dim = root.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.75f);
            dim.raycastTarget = true;
            root.SetActive(false);

            var center = new GameObject("DeathPanel");
            center.transform.SetParent(root.transform, false);
            var crt = center.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(520f, 300f);
            var panelBg = center.AddComponent<Image>();
            panelBg.color = new Color(0.12f, 0.12f, 0.14f, 0.98f);

            titleText = CreateHudTextLine(center.transform, "DeathTitle", "You died", 40, new Vector2(0f, 70f), font);
            countdownText = CreateHudTextLine(center.transform, "DeathCountdown", string.Empty, 22, new Vector2(0f, 10f), font);

            var btnGo = new GameObject("RespawnButton");
            btnGo.transform.SetParent(center.transform, false);
            var brt = btnGo.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0.5f);
            brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(0f, -85f);
            brt.sizeDelta = new Vector2(240f, 48f);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.25f, 0.45f, 0.85f, 1f);
            respawnButton = btnGo.AddComponent<Button>();
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(btnGo.transform, false);
            var lrt = labelGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var lt = labelGo.AddComponent<Text>();
            lt.font = font;
            lt.fontSize = 22;
            lt.color = Color.white;
            lt.alignment = TextAnchor.MiddleCenter;
            lt.text = "Respawn";
        }

        private static Text CreateHudTextLine(Transform parent, string name, string text, int fontSize, Vector2 anchoredPos,
            Font font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(480f, 56f);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = text;
            return t;
        }

        private static GameObject CreateGoblin(string id, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = id;
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.9f, 1.2f, 0.9f);
            SetColor(go, new Color(0.25f, 0.65f, 0.28f));
            var e = go.AddComponent<QuestSampleEnemy>();
            e.Configure(id, "Goblin");
            return go;
        }

        private static void CreatePickup(string id, Vector3 position, string itemName, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = id;
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.65f;
            SetColor(go, color);
            Object.Destroy(go.GetComponent<SphereCollider>());
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            var p = go.AddComponent<QuestSampleItemPickup>();
            p.Configure(id, itemName);
        }

        private static void CreateZone(string name, Vector3 center, Vector3 size, string zoneId)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var z = go.AddComponent<QuestSampleExplorationZone>();
            z.Configure(zoneId);
        }

        private static GameObject CreateNpcCube(string name, Vector3 position, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.1f, 2f, 1.1f);
            SetColor(go, color);
            return go;
        }

        private static void CreateLabelCube(string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            SetColor(go, color);
            Object.Destroy(go.GetComponent<BoxCollider>());
        }

        private static void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = color;
            }
        }

        private static Text CreateText(Transform parent, string name, int fontSize, TextAnchor align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
    }
}
