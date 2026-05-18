using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using GameplayMechanicsUMFOSS.Combat;
using GameplayMechanicsUMFOSS.Physics;
using GameplayMechanicsUMFOSS.Utils;
using GameplayMechanicsUMFOSS.Samples.BoomerangWeapon;

namespace GameplayMechanicsUMFOSS.Editor
{
    /// <summary>
    /// One-click demo scene builder. Creates the entire Boomerang Weapon demo
    /// with all GameObjects, components, config, materials, UI, and references wired up.
    /// Menu: UMF > Build Boomerang Demo Scene
    /// </summary>
    public static class BoomerangDemoSceneBuilder
    {
        private const string SCENE_PATH = "Assets/BoomerangDemo/RecallDemo.unity";
        private const string CONFIG_PATH = "Assets/BoomerangDemo/AxeConfig.asset";
        private const string SPEAR_CONFIG_PATH = "Assets/BoomerangDemo/SpearConfig.asset";

        private const int LAYER_HURTBOX = 6;
        private const int LAYER_STUCK = 7;

        [MenuItem("UMF/Build Boomerang Demo Scene")]
        public static void BuildScene()
        {
            SetupLayers();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            EnsureFolder("Assets", "BoomerangDemo");

            BoomerangConfig config = CreateOrLoadConfig();
            BoomerangConfig spearConfig = CreateOrLoadSpearConfig();

            // ─── Materials ───
            Material floorMat = MakeMat("FloorMat", new Color(0.35f, 0.35f, 0.38f));
            Material wallMat = MakeMat("WallMat", new Color(0.55f, 0.55f, 0.6f));
            Material dummyMat = MakeMat("DummyMat", new Color(0.9f, 0.3f, 0.2f));
            Material axeMat = MakeMat("AxeMat", new Color(0.7f, 0.75f, 0.8f));
            Material spearMat = MakeMat("SpearMat", new Color(0.6f, 0.6f, 0.65f));
            Material handleMat = MakeMat("HandleMat", new Color(0.4f, 0.25f, 0.12f));
            Material healthBarMat = MakeMat("HealthBarMat", new Color(0.1f, 0.9f, 0.2f));

            // ─── Lighting ───
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.95f, 0.85f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.35f);

            // ─── Environment ───
            Material platformMat = MakeMat("PlatformMat", new Color(0.45f, 0.42f, 0.38f));
            Material rampMat = MakeMat("RampMat", new Color(0.5f, 0.48f, 0.42f));
            Material obstacleMat = MakeMat("ObstacleMat", new Color(0.4f, 0.5f, 0.55f));
            Material movableMat = MakeMat("MovableMat", new Color(0.6f, 0.4f, 0.2f));
            Material patrolMat = MakeMat("PatrolMat", new Color(0.3f, 0.6f, 0.5f));

            // Ground floor — sized to match the play area
            // Content spans roughly x=[-45,45] z=[-35,55], floor covers that plus margin
            MakePrimitive("Floor", PrimitiveType.Plane, new Vector3(0f, 0f, 10f), new Vector3(10f, 1f, 10f), floorMat, LAYER_STUCK);

            // Perimeter walls (12 units high) — tight to the play area so player can't wander out of view
            // Play area: x = [-48, 48], z = [-38, 58]
            float wallHeight = 12f;
            float wallHalfH = wallHeight * 0.5f;
            // Back wall (z = 58)
            MakePrimitive("Wall_Back", PrimitiveType.Cube, new Vector3(0f, wallHalfH, 58f), new Vector3(96.5f, wallHeight, 0.5f), wallMat, LAYER_STUCK);
            // Front wall (z = -38)
            MakePrimitive("Wall_Front", PrimitiveType.Cube, new Vector3(0f, wallHalfH, -38f), new Vector3(96.5f, wallHeight, 0.5f), wallMat, LAYER_STUCK);
            // Left wall (x = -48)
            MakePrimitive("Wall_Left", PrimitiveType.Cube, new Vector3(-48f, wallHalfH, 10f), new Vector3(0.5f, wallHeight, 96.5f), wallMat, LAYER_STUCK);
            // Right wall (x = 48)
            MakePrimitive("Wall_Right", PrimitiveType.Cube, new Vector3(48f, wallHalfH, 10f), new Vector3(0.5f, wallHeight, 96.5f), wallMat, LAYER_STUCK);

            // ─── Level 1 West Platform (y=2) ───
            // Surface y = 2 + 0.25 = 2.25, front edge z = 20 - 6 = 14
            MakePrimitive("Platform_L1_West", PrimitiveType.Cube, new Vector3(-25f, 2f, 20f), new Vector3(16f, 0.5f, 12f), platformMat, LAYER_STUCK);
            MakeRamp("Ramp_L1_West",
                new Vector3(-25f, 0f, 7f),       // foot: ground level, 7 units in front of edge
                new Vector3(-25f, 2.25f, 14f),    // lip: flush with platform surface & front edge
                4f, rampMat);

            // ─── Level 2 West Platform (y=4) ───
            // Surface y = 4 + 0.25 = 4.25, front edge z = 35 - 4 = 31
            // L1 West back edge z = 20 + 6 = 26, surface y = 2.25
            MakePrimitive("Platform_L2_West", PrimitiveType.Cube, new Vector3(-25f, 4f, 35f), new Vector3(10f, 0.5f, 8f), platformMat, LAYER_STUCK);
            MakeRamp("Ramp_L2_West",
                new Vector3(-25f, 2.25f, 26f),    // foot: L1 West back edge, L1 surface level
                new Vector3(-25f, 4.25f, 31f),    // lip: flush with L2 surface & front edge
                3.5f, rampMat);

            // ─── Level 3 West Tower (y=6.5) ───
            // Surface y = 6.5 + 0.25 = 6.75, front edge z = 45 - 3 = 42
            // L2 West back edge z = 35 + 4 = 39, surface y = 4.25
            MakePrimitive("Platform_L3_West", PrimitiveType.Cube, new Vector3(-25f, 6.5f, 45f), new Vector3(7f, 0.5f, 6f), platformMat, LAYER_STUCK);
            MakeRamp("Ramp_L3_West",
                new Vector3(-25f, 4.25f, 39f),    // foot: L2 back edge, L2 surface level
                new Vector3(-25f, 6.75f, 42f),    // lip: flush with L3 surface & front edge
                3f, rampMat);

            // ─── Level 1 East Platform (y=2.5) ───
            // Surface y = 2.5 + 0.25 = 2.75, front edge z = 20 - 6 = 14
            MakePrimitive("Platform_L1_East", PrimitiveType.Cube, new Vector3(25f, 2.5f, 20f), new Vector3(14f, 0.5f, 12f), platformMat, LAYER_STUCK);
            MakeRamp("Ramp_L1_East",
                new Vector3(25f, 0f, 6f),         // foot: ground level
                new Vector3(25f, 2.75f, 14f),     // lip: flush with platform surface & front edge
                4f, rampMat);

            // ─── Level 2 East Platform (y=5) ───
            // Surface y = 5 + 0.25 = 5.25, front edge z = 35 - 4 = 31
            // L1 East back edge z = 20 + 6 = 26, surface y = 2.75
            MakePrimitive("Platform_L2_East", PrimitiveType.Cube, new Vector3(25f, 5f, 35f), new Vector3(10f, 0.5f, 8f), platformMat, LAYER_STUCK);
            MakeRamp("Ramp_L2_East",
                new Vector3(25f, 2.75f, 26f),     // foot: L1 East back edge, L1 surface level
                new Vector3(25f, 5.25f, 31f),     // lip: flush with L2 surface & front edge
                3.5f, rampMat);

            // ─── Central Bridge (y=3.5, connects west and east) ───
            MakePrimitive("Bridge_Central", PrimitiveType.Cube, new Vector3(0f, 3.5f, 27f), new Vector3(2.5f, 0.3f, 18f), platformMat, LAYER_STUCK);

            // ─── South Arena Platform (y=1.5) ───
            // Surface y = 1.5 + 0.25 = 1.75, front edge z = -20 - 5 = -25
            MakePrimitive("Platform_South", PrimitiveType.Cube, new Vector3(0f, 1.5f, -20f), new Vector3(12f, 0.5f, 10f), platformMat, LAYER_STUCK);
            MakeRamp("Ramp_South",
                new Vector3(0f, 0f, -31f),        // foot: ground level
                new Vector3(0f, 1.75f, -25f),     // lip: flush with platform surface & front edge
                4f, rampMat);

            // ─── Static Obstacles (pillars, cubes, cylinders) ───
            // Ground level
            MakePrimitive("Pillar_01", PrimitiveType.Cube, new Vector3(5f, 1.5f, 8f), new Vector3(1f, 3f, 1f), wallMat, LAYER_STUCK);
            MakePrimitive("Pillar_02", PrimitiveType.Cube, new Vector3(-8f, 2f, 12f), new Vector3(1.2f, 4f, 1.2f), wallMat, LAYER_STUCK);
            MakePrimitive("Pillar_03", PrimitiveType.Cube, new Vector3(12f, 1f, 5f), new Vector3(0.8f, 2f, 0.8f), wallMat, LAYER_STUCK);
            MakePrimitive("Pillar_04", PrimitiveType.Cube, new Vector3(-5f, 1.5f, 30f), new Vector3(1f, 3f, 1f), wallMat, LAYER_STUCK);
            MakePrimitive("Pillar_05", PrimitiveType.Cube, new Vector3(0f, 2.5f, 15f), new Vector3(1.5f, 5f, 1.5f), wallMat, LAYER_STUCK);
            MakePrimitive("Pillar_06", PrimitiveType.Cube, new Vector3(-15f, 2f, 5f), new Vector3(1f, 4f, 1f), wallMat, LAYER_STUCK);
            MakePrimitive("Pillar_07", PrimitiveType.Cube, new Vector3(18f, 1.5f, 40f), new Vector3(1.2f, 3f, 1.2f), wallMat, LAYER_STUCK);
            MakePrimitive("Pillar_08", PrimitiveType.Cube, new Vector3(-35f, 2f, 10f), new Vector3(1f, 4f, 1f), wallMat, LAYER_STUCK);
            // Cube obstacles
            MakePrimitive("Obstacle_Cube_01", PrimitiveType.Cube, new Vector3(8f, 0.75f, 18f), new Vector3(1.5f, 1.5f, 1.5f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_Cube_02", PrimitiveType.Cube, new Vector3(-12f, 0.75f, 6f), new Vector3(2f, 1.5f, 1f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_Cube_03", PrimitiveType.Cube, new Vector3(30f, 0.75f, 8f), new Vector3(1.5f, 1.5f, 2f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_Cube_04", PrimitiveType.Cube, new Vector3(-30f, 0.75f, 40f), new Vector3(2f, 1.5f, 2f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_Cube_05", PrimitiveType.Cube, new Vector3(10f, 0.5f, -15f), new Vector3(3f, 1f, 1f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_Cube_06", PrimitiveType.Cube, new Vector3(-10f, 0.5f, -18f), new Vector3(1f, 1f, 3f), obstacleMat, LAYER_STUCK);
            // Cylinder obstacles
            MakePrimitive("Obstacle_Cylinder_01", PrimitiveType.Cylinder, new Vector3(15f, 1f, 22f), new Vector3(1f, 1f, 1f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_Cylinder_02", PrimitiveType.Cylinder, new Vector3(-3f, 1f, 38f), new Vector3(1.2f, 1f, 1.2f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_Cylinder_03", PrimitiveType.Cylinder, new Vector3(35f, 1.5f, 30f), new Vector3(1.5f, 1.5f, 1.5f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_Cylinder_04", PrimitiveType.Cylinder, new Vector3(-40f, 1f, 25f), new Vector3(1f, 1f, 1f), obstacleMat, LAYER_STUCK);
            // Platform-level obstacles
            MakePrimitive("Obstacle_PlatW_01", PrimitiveType.Cube, new Vector3(-22f, 3f, 18f), new Vector3(1f, 2f, 1f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_PlatE_01", PrimitiveType.Cube, new Vector3(28f, 3.5f, 22f), new Vector3(1f, 2f, 1f), obstacleMat, LAYER_STUCK);
            MakePrimitive("Obstacle_PlatE_02", PrimitiveType.Cylinder, new Vector3(22f, 3.5f, 17f), new Vector3(0.8f, 1f, 0.8f), obstacleMat, LAYER_STUCK);
            // Walls/cover
            MakePrimitive("Cover_Wall_01", PrimitiveType.Cube, new Vector3(0f, 1f, 45f), new Vector3(6f, 2f, 0.5f), wallMat, LAYER_STUCK);
            MakePrimitive("Cover_Wall_02", PrimitiveType.Cube, new Vector3(15f, 1f, -10f), new Vector3(0.5f, 2f, 4f), wallMat, LAYER_STUCK);
            MakePrimitive("Cover_Wall_03", PrimitiveType.Cube, new Vector3(-20f, 1f, -10f), new Vector3(0.5f, 2f, 6f), wallMat, LAYER_STUCK);

            // ─── Movable Obstacles (physics-driven, axe sticks + pushes) ───
            BuildMovableObstacle("Movable_Crate_01", new Vector3(3f, 0.6f, 14f), new Vector3(1.2f, 1.2f, 1.2f), movableMat);
            BuildMovableObstacle("Movable_Crate_02", new Vector3(-6f, 0.6f, 25f), new Vector3(1f, 1f, 1f), movableMat);
            BuildMovableObstacle("Movable_Crate_03", new Vector3(10f, 0.6f, 10f), new Vector3(0.8f, 0.8f, 0.8f), movableMat);
            BuildMovableObstacle("Movable_Crate_04", new Vector3(-15f, 0.6f, 35f), new Vector3(1.5f, 1.5f, 1.5f), movableMat);
            BuildMovableObstacle("Movable_Crate_05", new Vector3(20f, 0.6f, 5f), new Vector3(1f, 1f, 1.5f), movableMat);
            BuildMovableObstacle("Movable_Barrel_01", new Vector3(5f, 0.5f, -12f), new Vector3(1f, 1f, 1f), movableMat);

            // ─── Patrolling Obstacles (moving platforms, axe rides along) ───
            BuildPatrolObstacle("Patrol_Platform_01",
                new Vector3(-10f, 1.5f, 15f), new Vector3(-10f, 1.5f, 30f),
                new Vector3(2f, 0.4f, 2f), 1.2f, patrolMat);
            BuildPatrolObstacle("Patrol_Platform_02",
                new Vector3(15f, 3f, 20f), new Vector3(30f, 3f, 20f),
                new Vector3(2.5f, 0.4f, 2.5f), 1.5f, patrolMat);
            BuildPatrolObstacle("Patrol_Wall_01",
                new Vector3(0f, 1f, 50f), new Vector3(15f, 1f, 50f),
                new Vector3(1f, 2f, 0.5f), 1f, patrolMat);
            BuildPatrolObstacle("Patrol_Platform_03",
                new Vector3(-35f, 2f, 15f), new Vector3(-35f, 2f, 35f),
                new Vector3(3f, 0.4f, 3f), 1f, patrolMat);
            BuildPatrolObstacle("Patrol_Elevator_01",
                new Vector3(40f, 0.5f, 25f), new Vector3(40f, 5f, 25f),
                new Vector3(2.5f, 0.3f, 2.5f), 0.8f, patrolMat);

            // ─── Player ───
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0f, 0f);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 1f, 0f);

            var camHolder = CreateChild("CameraHolder", player, new Vector3(0f, 1.6f, 0f));

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(camHolder.transform);
            camGo.transform.localPosition = Vector3.zero;
            camGo.transform.localRotation = Quaternion.identity;
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.fieldOfView = 70f;
            cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.AddComponent<AudioListener>();
            var cameraShake = camGo.AddComponent<CameraShake>();

            var handSocket = CreateChild("HandSocket", camHolder, new Vector3(0.4f, -0.3f, 0.8f));

            // PlayerController
            var playerCtrl = player.AddComponent<PlayerController>();
            var pcSO = new SerializedObject(playerCtrl);
            pcSO.FindProperty("cameraHolder").objectReferenceValue = camHolder.transform;
            pcSO.ApplyModifiedPropertiesWithoutUndo();

            // ─── Weapon (Axe) ───
            // Root object: physics + logic only (no renderer)
            var axe = new GameObject("Axe");
            axe.transform.SetParent(handSocket.transform);
            axe.transform.localPosition = Vector3.zero;
            axe.transform.localRotation = Quaternion.identity;

            var axeCollider = axe.AddComponent<BoxCollider>();
            axeCollider.size = new Vector3(0.15f, 0.5f, 0.05f);
            axeCollider.enabled = false; // starts disabled, EquippedState controls this

            var axeRb = axe.AddComponent<Rigidbody>();
            axeRb.isKinematic = true;
            axeRb.useGravity = false;

            axe.AddComponent<Physics3DAdapter>();

            // Visual pivot: child that spins during flight
            var visualPivot = CreateChild("VisualPivot", axe, Vector3.zero);

            // Axe blade mesh (under visual pivot so it spins)
            var bladeMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bladeMesh.name = "Blade";
            bladeMesh.transform.SetParent(visualPivot.transform);
            bladeMesh.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            bladeMesh.transform.localRotation = Quaternion.identity;
            bladeMesh.transform.localScale = new Vector3(0.15f, 0.3f, 0.05f);
            bladeMesh.GetComponent<Renderer>().sharedMaterial = axeMat;
            Object.DestroyImmediate(bladeMesh.GetComponent<BoxCollider>()); // renderer only

            // Handle mesh (under visual pivot)
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(visualPivot.transform);
            handle.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            handle.transform.localRotation = Quaternion.identity;
            handle.transform.localScale = new Vector3(0.06f, 0.35f, 0.06f);
            handle.GetComponent<Renderer>().sharedMaterial = handleMat;
            Object.DestroyImmediate(handle.GetComponent<BoxCollider>()); // renderer only

            var axeWeapon = axe.AddComponent<BoomerangWeapon_UMFOSS>();
            SetWeaponFields(axeWeapon, config, handSocket.transform, visualPivot.transform, axeCollider);

            // ─── Weapon (Spear) ───
            var spear = new GameObject("Spear");
            spear.transform.SetParent(handSocket.transform);
            spear.transform.localPosition = Vector3.zero;
            spear.transform.localRotation = Quaternion.identity;
            spear.SetActive(false); // starts inactive, axe is default

            var spearCollider = spear.AddComponent<CapsuleCollider>();
            spearCollider.radius = 0.04f;
            spearCollider.height = 0.9f;
            spearCollider.direction = 2; // Z-axis (forward, matches throw direction)
            spearCollider.enabled = false;

            var spearRb = spear.AddComponent<Rigidbody>();
            spearRb.isKinematic = true;
            spearRb.useGravity = false;

            spear.AddComponent<Physics3DAdapter>();

            var spearVisualPivot = CreateChild("VisualPivot", spear, Vector3.zero);

            // Spear shaft (long thin cylinder, rotated so length runs along local Z = forward)
            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Shaft";
            shaft.transform.SetParent(spearVisualPivot.transform);
            shaft.transform.localPosition = Vector3.zero;
            shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shaft.transform.localScale = new Vector3(0.04f, 0.4f, 0.04f);
            shaft.GetComponent<Renderer>().sharedMaterial = handleMat;
            Object.DestroyImmediate(shaft.GetComponent<CapsuleCollider>());

            // Spear tip (diamond shape at the front along Z)
            var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tip.name = "Tip";
            tip.transform.SetParent(spearVisualPivot.transform);
            tip.transform.localPosition = new Vector3(0f, 0f, 0.42f);
            tip.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            tip.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
            tip.GetComponent<Renderer>().sharedMaterial = spearMat;
            Object.DestroyImmediate(tip.GetComponent<BoxCollider>());

            var spearWeapon = spear.AddComponent<BoomerangWeapon_UMFOSS>();
            SetWeaponFields(spearWeapon, spearConfig, handSocket.transform, spearVisualPivot.transform, spearCollider);

            // ─── Demo Controller ───
            var demoCtrl = player.AddComponent<BoomerangDemoController>();
            var dcSO = new SerializedObject(demoCtrl);
            dcSO.FindProperty("weapons").arraySize = 2;
            dcSO.FindProperty("weapons").GetArrayElementAtIndex(0).objectReferenceValue = axeWeapon;
            dcSO.FindProperty("weapons").GetArrayElementAtIndex(1).objectReferenceValue = spearWeapon;
            dcSO.FindProperty("playerCamera").objectReferenceValue = cam;
            dcSO.FindProperty("aimLayers").intValue = 1 << LAYER_HURTBOX;
            dcSO.ApplyModifiedPropertiesWithoutUndo();

            // ─── Target Dummies (10 total, spread across levels) ───
            // Ground level (large 3D bounds)
            BuildDummy("Dummy_01", new Vector3(0f, 2f, 7f), dummyMat, healthBarMat, 1.3f, new Vector3(5f, 3f, 5f));
            BuildDummy("Dummy_02", new Vector3(8f, 2f, 15f), dummyMat, healthBarMat, 1.8f, new Vector3(6f, 3f, 6f));
            BuildDummy("Dummy_03", new Vector3(-10f, 2f, 10f), dummyMat, healthBarMat, 1.1f, new Vector3(5f, 2f, 5f));
            BuildDummy("Dummy_04", new Vector3(0f, 2f, -15f), dummyMat, healthBarMat, 1.5f, new Vector3(4f, 2f, 4f));
            // Level 1 West platform
            BuildDummy("Dummy_05", new Vector3(-22f, 4f, 20f), dummyMat, healthBarMat, 1.4f, new Vector3(4f, 2f, 3f));
            BuildDummy("Dummy_06", new Vector3(-28f, 4f, 18f), dummyMat, healthBarMat, 1.2f, new Vector3(3f, 2f, 3f));
            // Level 1 East platform
            BuildDummy("Dummy_07", new Vector3(22f, 4.5f, 18f), dummyMat, healthBarMat, 1.6f, new Vector3(4f, 2f, 4f));
            BuildDummy("Dummy_08", new Vector3(28f, 4.5f, 22f), dummyMat, healthBarMat, 1.0f, new Vector3(3f, 2f, 3f));
            // Level 2 platforms
            BuildDummy("Dummy_09", new Vector3(-25f, 6f, 35f), dummyMat, healthBarMat, 1.3f, new Vector3(3f, 2f, 2f));
            BuildDummy("Dummy_10", new Vector3(25f, 7f, 35f), dummyMat, healthBarMat, 1.5f, new Vector3(3f, 2f, 2f));

            // ─── UI ───
            BuildUI(camGo, demoCtrl);

            // ─── Feedback Systems ───
            var feedbackGo = new GameObject("FeedbackSystems");

            var wfh = feedbackGo.AddComponent<WeaponFeedbackHandler>();
            var wfhSO = new SerializedObject(wfh);
            wfhSO.FindProperty("cameraShake").objectReferenceValue = cameraShake;
            wfhSO.ApplyModifiedPropertiesWithoutUndo();

            feedbackGo.AddComponent<ImpactEffectSpawner>();

            // ─── Save ───
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;
            SceneView.lastActiveSceneView?.FrameSelected();

            Debug.Log("<color=green><b>[UMF] Boomerang Demo Scene built!</b></color>\n" +
                      "Press <b>Play</b>, then click the Game view to lock cursor.\n" +
                      "<b>Controls:</b> WASD move | Shift sprint | Space jump | Mouse look | Left-Click throw | R recall | Q switch weapon | Esc unlock cursor");
        }

        // ═══════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════

        private static void SetupLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var layers = tagManager.FindProperty("layers");

            SetLayerIfEmpty(layers, LAYER_HURTBOX, "Hurtbox");
            SetLayerIfEmpty(layers, LAYER_STUCK, "Stuck");

            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetLayerIfEmpty(SerializedProperty layers, int index, string name)
        {
            var layer = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layer.stringValue))
                layer.stringValue = name;
        }

        private static BoomerangConfig CreateOrLoadConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<BoomerangConfig>(CONFIG_PATH);
            if (existing != null) return existing;

            var config = ScriptableObject.CreateInstance<BoomerangConfig>();
            var so = new SerializedObject(config);
            so.FindProperty("throwForce").floatValue = 30f;
            so.FindProperty("maxRange").floatValue = 25f;
            so.FindProperty("returnSpeed").floatValue = 15f;
            so.FindProperty("returnAcceleration").floatValue = 2f;
            so.FindProperty("catchDistance").floatValue = 0.5f;
            so.FindProperty("arcHeightRatio").floatValue = 0.3f;
            so.FindProperty("arcSideRatio").floatValue = 0.15f;
            so.FindProperty("throwDamage").floatValue = 25f;
            so.FindProperty("returnDamage").floatValue = 15f;
            so.FindProperty("knockbackForce").floatValue = 8f;
            so.FindProperty("hurtboxLayer").intValue = 1 << LAYER_HURTBOX;
            so.FindProperty("stuckLayer").intValue = 1 << LAYER_STUCK;
            so.FindProperty("embedInTargets").boolValue = true;
            so.FindProperty("rotationPerSecond").vector3Value = new Vector3(0f, 0f, 1080f);
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(config, CONFIG_PATH);
            return config;
        }

        private static BoomerangConfig CreateOrLoadSpearConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<BoomerangConfig>(SPEAR_CONFIG_PATH);
            if (existing != null) return existing;

            var config = ScriptableObject.CreateInstance<BoomerangConfig>();
            var so = new SerializedObject(config);
            so.FindProperty("throwForce").floatValue = 40f;
            so.FindProperty("maxRange").floatValue = 35f;
            so.FindProperty("returnSpeed").floatValue = 18f;
            so.FindProperty("returnAcceleration").floatValue = 3f;
            so.FindProperty("catchDistance").floatValue = 0.5f;
            so.FindProperty("arcHeightRatio").floatValue = 0.15f;
            so.FindProperty("arcSideRatio").floatValue = 0.05f;
            so.FindProperty("throwDamage").floatValue = 35f;
            so.FindProperty("returnDamage").floatValue = 10f;
            so.FindProperty("knockbackForce").floatValue = 12f;
            so.FindProperty("hurtboxLayer").intValue = 1 << LAYER_HURTBOX;
            so.FindProperty("stuckLayer").intValue = 1 << LAYER_STUCK;
            so.FindProperty("embedInTargets").boolValue = true;
            so.FindProperty("rotationPerSecond").vector3Value = new Vector3(0f, 0f, 0f);
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(config, SPEAR_CONFIG_PATH);
            return config;
        }

        private static void SetWeaponFields(BoomerangWeapon_UMFOSS weapon, BoomerangConfig config,
            Transform handSocket, Transform visualPivot, Collider col)
        {
            var so = new SerializedObject(weapon);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("handSocket").objectReferenceValue = handSocket;
            so.FindProperty("visualPivot").objectReferenceValue = visualPivot;
            so.FindProperty("weaponCollider").objectReferenceValue = col;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildDummy(string name, Vector3 position, Material dummyMat, Material healthBarMat,
            float wanderSpeed = 1.5f, Vector3? bounds = null)
        {
            Vector3 boundsExtents = bounds ?? new Vector3(4f, 2f, 4f);
            var dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = name;
            dummy.transform.position = position;
            dummy.layer = LAYER_HURTBOX;
            dummy.GetComponent<Renderer>().sharedMaterial = dummyMat;

            var rb = dummy.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = true;

            var damageable = dummy.AddComponent<Damageable>();
            var dmgSO = new SerializedObject(damageable);
            dmgSO.FindProperty("maxHealth").floatValue = 50f;
            dmgSO.FindProperty("knockbackRb").objectReferenceValue = rb;
            dmgSO.FindProperty("hitFlashRenderer").objectReferenceValue = dummy.GetComponent<Renderer>();
            dmgSO.FindProperty("hitFlashColor").colorValue = Color.white;
            dmgSO.FindProperty("hitFlashDuration").floatValue = 0.15f;
            dmgSO.ApplyModifiedPropertiesWithoutUndo();

            // Health bar
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "HealthBar";
            bar.transform.SetParent(dummy.transform);
            bar.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            bar.transform.localScale = new Vector3(0.8f, 0.08f, 0.08f);
            bar.layer = 0;
            bar.GetComponent<Renderer>().sharedMaterial = healthBarMat;
            Object.DestroyImmediate(bar.GetComponent<BoxCollider>());

            var td = dummy.AddComponent<TargetDummy>();
            var tdSO = new SerializedObject(td);
            tdSO.FindProperty("respawnDelay").floatValue = 3f;
            tdSO.FindProperty("ragdollRb").objectReferenceValue = rb;
            tdSO.FindProperty("healthBarFill").objectReferenceValue = bar.transform;
            tdSO.ApplyModifiedPropertiesWithoutUndo();

            var wanderer = dummy.AddComponent<TargetWanderer>();
            var wSO = new SerializedObject(wanderer);
            wSO.FindProperty("moveSpeed").floatValue = wanderSpeed;
            wSO.FindProperty("directionChangeMin").floatValue = 1f;
            wSO.FindProperty("directionChangeMax").floatValue = 3f;
            wSO.FindProperty("boundsExtents").vector3Value = boundsExtents;
            wSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildUI(GameObject camGo, BoomerangDemoController demoCtrl)
        {
            var canvasGo = new GameObject("UI Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Crosshair dot
            var crosshairGo = new GameObject("Crosshair");
            crosshairGo.transform.SetParent(canvasGo.transform, false);
            var crossRect = crosshairGo.AddComponent<RectTransform>();
            crossRect.anchorMin = crossRect.anchorMax = new Vector2(0.5f, 0.5f);
            crossRect.anchoredPosition = Vector2.zero;
            crossRect.sizeDelta = new Vector2(6f, 6f);
            var crossImg = crosshairGo.AddComponent<UnityEngine.UI.Image>();
            crossImg.color = Color.white;

            // State label (top center)
            var stateGo = MakeTMP("StateLabel", canvasGo.transform,
                new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(300f, 40f),
                "EQUIPPED", 24, TextAlignmentOptions.Center, Color.white);

            // Controls label (bottom center)
            var controlsGo = MakeTMP("ControlsLabel", canvasGo.transform,
                new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(800f, 30f),
                "LMB: Throw  |  R: Recall  |  Q: Switch  |  WASD: Move  |  Shift: Sprint  |  Space: Jump  |  Esc: Unlock",
                16, TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.7f));

            // Wire UI to demo controller
            var dcSO = new SerializedObject(demoCtrl);
            dcSO.FindProperty("stateLabel").objectReferenceValue = stateGo.GetComponent<TextMeshProUGUI>();
            dcSO.FindProperty("controlsLabel").objectReferenceValue = controlsGo.GetComponent<TextMeshProUGUI>();
            dcSO.FindProperty("crosshair").objectReferenceValue = crossRect;
            dcSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject MakeTMP(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size,
            string text, int fontSize, TextAlignmentOptions align, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            return go;
        }

        /// <summary>
        /// Creates a ramp (rotated cube) connecting two points.
        /// footPos: where the bottom of the ramp surface touches (ground/lower platform).
        /// lipPos: where the top of the ramp surface meets the upper platform edge.
        /// The ramp's top surface passes exactly through both points.
        /// </summary>
        private static void MakeRamp(string name, Vector3 footPos, Vector3 lipPos, float width, Material mat)
        {
            const float THICKNESS = 0.3f;

            Vector3 along = lipPos - footPos;
            float rampLength = along.magnitude;

            // Build rotation: the cube's local +Z axis should point from foot to lip
            Quaternion rotation = Quaternion.LookRotation(along.normalized, Vector3.up);

            // The cube's top surface (local +Y) is THICKNESS/2 above its center.
            // We want the top surface to pass through the foot-lip line, so shift the
            // center downward in the ramp's local up direction by half the thickness.
            Vector3 localUp = rotation * Vector3.up;
            Vector3 midpoint = (footPos + lipPos) * 0.5f;
            Vector3 center = midpoint - localUp * (THICKNESS * 0.5f);

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = center;
            go.transform.rotation = rotation;
            go.transform.localScale = new Vector3(width, THICKNESS, rampLength);
            go.layer = LAYER_STUCK;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void BuildMovableObstacle(string name, Vector3 position, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            go.layer = LAYER_STUCK;
            go.GetComponent<Renderer>().sharedMaterial = mat;

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 5f;
            rb.linearDamping = 2f;
            rb.angularDamping = 1f;
            rb.useGravity = true;
        }

        private static void BuildPatrolObstacle(string name, Vector3 pointA, Vector3 pointB,
            Vector3 scale, float speed, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pointA;
            go.transform.localScale = scale;
            go.layer = LAYER_STUCK;
            go.GetComponent<Renderer>().sharedMaterial = mat;

            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var patroller = go.AddComponent<ObstaclePatroller>();
            var so = new SerializedObject(patroller);
            so.FindProperty("pointA").vector3Value = pointA;
            so.FindProperty("pointB").vector3Value = pointB;
            so.FindProperty("speed").floatValue = speed;
            so.FindProperty("pauseAtEnds").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject MakePrimitive(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat, int layer)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.layer = layer;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateChild(string name, GameObject parent, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        /// <summary>
        /// Creates a material using the first available shader across render pipelines.
        /// Tries URP Lit, then HDRP Lit, then Built-in Standard.
        /// </summary>
        private static Material MakeMat(string name, Color color)
        {
            // Try URP first (most common modern pipeline)
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader != null)
            {
                var mat = new Material(shader);
                mat.name = name;
                mat.SetColor("_BaseColor", color);
                return mat;
            }

            // Try HDRP
            shader = Shader.Find("HDRP/Lit");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.name = name;
                mat.SetColor("_BaseColor", color);
                return mat;
            }

            // Fall back to Built-in Standard
            shader = Shader.Find("Standard");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.name = name;
                mat.color = color;
                return mat;
            }

            // Last resort: use Unity's default material as base
            var fallback = new Material(Shader.Find("Sprites/Default"));
            fallback.name = name;
            fallback.color = color;
            Debug.LogWarning($"[UMF] Could not find a lit shader for material '{name}'. Using fallback.");
            return fallback;
        }

        private static void EnsureFolder(string parent, string folder)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + folder))
                AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
