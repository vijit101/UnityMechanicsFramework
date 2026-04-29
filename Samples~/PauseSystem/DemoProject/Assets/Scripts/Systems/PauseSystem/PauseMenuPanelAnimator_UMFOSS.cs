using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;

public class PauseMenuPanelAnimator_UMFOSS : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private Vector2 hiddenAnchoredPosition = new Vector2(0f, -900f);
    [SerializeField] private Vector2 visibleAnchoredPosition = Vector2.zero;
    [SerializeField] private float moveSpeed = 12f;

    private bool shouldBeVisible;

    private void Awake()
    {
        if (panel == null)
        {
            panel = GetComponent<RectTransform>();
        }

        if (panel != null)
        {
            panel.anchoredPosition = hiddenAnchoredPosition;
        }
    }

    private void OnEnable()
    {
        GameplayMechanicsUMFOSS.Core.EventBus.Subscribe<GamePausedEvent>(OnPaused);
        GameplayMechanicsUMFOSS.Core.EventBus.Subscribe<GameResumedEvent>(OnResumed);
    }

    private void OnDisable()
    {
        GameplayMechanicsUMFOSS.Core.EventBus.Unsubscribe<GamePausedEvent>(OnPaused);
        GameplayMechanicsUMFOSS.Core.EventBus.Unsubscribe<GameResumedEvent>(OnResumed);
    }

    private void Update()
    {
        if (panel == null)
        {
            return;
        }

        Vector2 target = shouldBeVisible ? visibleAnchoredPosition : hiddenAnchoredPosition;
        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, target, moveSpeed * Time.unscaledDeltaTime);
    }

    private void OnPaused(GamePausedEvent _)
    {
        shouldBeVisible = true;
    }

    private void OnResumed(GameResumedEvent _)
    {
        shouldBeVisible = false;
    }
}
