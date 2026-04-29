using GameplayMechanicsUMFOSS.Systems;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PauseSystemDemoSceneBuilder_UMFOSS
{
    [MenuItem("Tools/UMFOSS/Pause System/Build Demo Scene")]
    public static void BuildDemoScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "DemoScene";

        CreateEnvironment();

        GameObject pauseSystemObject = new GameObject("PauseSystem");
        pauseSystemObject.AddComponent<PauseSystem_UMFOSS>();

        GameObject controllerObject = new GameObject("DemoController");
        PauseSystemDemoController_UMFOSS controller = controllerObject.AddComponent<PauseSystemDemoController_UMFOSS>();

        GameObject canvasObject = CreateCanvasWithEventSystem();
        RectTransform canvasRoot = canvasObject.GetComponent<RectTransform>();

        Text timeScaleText = CreateLabel("TimeScaleText", canvasRoot, new Vector2(210f, -40f));
        Text isPausedText = CreateLabel("IsPausedText", canvasRoot, new Vector2(210f, -70f));
        Text storedTimeScaleText = CreateLabel("StoredTimeScaleText", canvasRoot, new Vector2(210f, -100f));
        Text audioPauseText = CreateLabel("AudioPauseText", canvasRoot, new Vector2(210f, -130f));
        Text lastEventText = CreateLabel("LastEventText", canvasRoot, new Vector2(260f, -160f));

        Button slowMoButton = CreateButton("SlowMoButton", canvasRoot, "Activate Slow Mo", new Vector2(120f, -220f));
        Button pauseButton = CreateButton("PauseButton", canvasRoot, "Pause", new Vector2(120f, -265f));
        Button resumeButton = CreateButton("ResumeButton", canvasRoot, "Resume", new Vector2(120f, -310f));

        GameObject pausePanel = CreatePausePanel(canvasRoot, controller);
        pausePanel.AddComponent<PauseMenuPanelAnimator_UMFOSS>();

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("timeScaleText").objectReferenceValue = timeScaleText;
        serializedController.FindProperty("isPausedText").objectReferenceValue = isPausedText;
        serializedController.FindProperty("storedTimeScaleText").objectReferenceValue = storedTimeScaleText;
        serializedController.FindProperty("audioPauseText").objectReferenceValue = audioPauseText;
        serializedController.FindProperty("lastEventText").objectReferenceValue = lastEventText;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(slowMoButton.onClick, controller.ActivateSlowMo);
        UnityEventTools.AddPersistentListener(pauseButton.onClick, controller.PauseFromButton);
        UnityEventTools.AddPersistentListener(resumeButton.onClick, controller.ResumeFromButton);

        string scenePath = "Assets/Scenes/DemoScene.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Pause Demo", "Demo scene created at Assets/Scenes/DemoScene.unity", "OK");
    }

    private static void CreateEnvironment()
    {
        GameObject camera = new GameObject("Main Camera");
        Camera cam = camera.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 3f, -10f);
        camera.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
        cam.clearFlags = CameraClearFlags.Skybox;

        GameObject light = new GameObject("Directional Light");
        Light directional = light.AddComponent<Light>();
        directional.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;

        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = "EnemyMover";
        enemy.transform.position = new Vector3(0f, 0.5f, 2f);
        enemy.AddComponent<SimpleMover_UMFOSS>();

        GameObject rotator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rotator.name = "Rotator";
        rotator.transform.position = new Vector3(0f, 1f, -1.5f);
        rotator.AddComponent<SimpleRotator_UMFOSS>();

        GameObject particles = new GameObject("Particles");
        particles.transform.position = new Vector3(-2f, 0f, 0f);
        particles.AddComponent<ParticleSystem>();

        GameObject music = new GameObject("BackgroundMusic");
        AudioSource source = music.AddComponent<AudioSource>();
        source.playOnAwake = true;
        source.loop = true;
        source.spatialBlend = 0f;
    }

    private static GameObject CreateCanvasWithEventSystem()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        return canvasObject;
    }

    private static Text CreateLabel(string name, RectTransform parent, Vector2 anchoredPosition)
    {
        GameObject label = new GameObject(name);
        label.transform.SetParent(parent, false);
        RectTransform rt = label.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = new Vector2(500f, 28f);

        Text text = label.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 20;
        text.color = Color.white;
        text.text = name;
        return text;
    }

    private static Button CreateButton(string name, RectTransform parent, string title, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rt = buttonObject.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = new Vector2(220f, 34f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        button.colors = colors;

        Text label = CreateLabel(name + "_Label", rt, new Vector2(8f, -6f));
        label.alignment = TextAnchor.MiddleLeft;
        label.text = title;
        return button;
    }

    private static GameObject CreatePausePanel(RectTransform canvasRoot, PauseSystemDemoController_UMFOSS controller)
    {
        GameObject panel = new GameObject("PausePanel");
        panel.transform.SetParent(canvasRoot, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(380f, 220f);
        rt.anchoredPosition = new Vector2(0f, -900f);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.78f);

        Text title = CreateLabel("PauseTitle", rt, new Vector2(-120f, 92f));
        title.alignment = TextAnchor.MiddleCenter;
        title.text = "Paused";
        title.fontSize = 28;

        Button resume = CreateButton("PausePanel_Resume", rt, "Resume", new Vector2(-110f, 20f));
        Button quit = CreateButton("PausePanel_Quit", rt, "Quit", new Vector2(-110f, 70f));
        UnityEventTools.AddPersistentListener(resume.onClick, controller.ResumeFromButton);
        UnityEventTools.AddPersistentListener(quit.onClick, controller.QuitApplication);
        return panel;
    }
}
