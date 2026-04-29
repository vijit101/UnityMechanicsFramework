using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Optional loading screen UI. Listens to SceneLoadProgressEvent on the EventBus
    /// and updates a progress bar. Never references SceneManager_UMFOSS directly.
    /// </summary>
    public class LoadingScreen_UMFOSS : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI tipText;

        [Header("Tips")]
        [SerializeField] private string[] loadingTips;

        private void OnEnable()
        {
            EventBus.Subscribe<SceneLoadProgressEvent>(OnProgress);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SceneLoadProgressEvent>(OnProgress);
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            if (tipText != null && loadingTips != null && loadingTips.Length > 0)
                tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            if (progressBar != null) progressBar.value = 0f;
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void OnProgress(SceneLoadProgressEvent e)
        {
            if (progressBar != null) progressBar.value = e.progress;
        }
    }
}
