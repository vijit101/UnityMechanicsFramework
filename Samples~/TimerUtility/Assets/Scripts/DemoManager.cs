using UnityEngine;
using UnityEngine.UI;
using GameplayMechanicsUMFOSS.Samples.TimerUtility;

/// <summary>
/// Bootstraps all 4 timer demos. Attach to one empty GameObject, press Play — done!
/// </summary>
public class DemoManager : MonoBehaviour
{
    private void Awake()
    {
        // Create Canvas with proper scaling
        var canvasGO = new GameObject("MainCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem — required for UI button clicks
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Add a dark background panel
        AddBackground(canvas.transform);

        // Spawn all 4 demos
        new GameObject("Demo1_CooldownTimer").AddComponent<CooldownTimerDemo>();
        new GameObject("Demo2_CountdownDisplay").AddComponent<CountdownDisplayDemo>();
        new GameObject("Demo3_LoopingSpawn").AddComponent<LoopingSpawnDemo>();
        new GameObject("Demo4_PausedTimer").AddComponent<PausedTimerDemo>();

        Debug.Log("[DemoManager] All 4 TimerUtility demos initialised!");
    }

    private void AddBackground(Transform parent)
    {
        var bg   = new GameObject("Background");
        bg.transform.SetParent(parent, false);
        var img  = bg.AddComponent<Image>();
        img.color = new Color(0.1f, 0.12f, 0.18f, 1f);
        var rect = bg.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        bg.transform.SetAsFirstSibling(); // move behind all other UI
    }
}
