using System.Collections;
using GameplayMechanicsUMFOSS.Utils;
using TMPro;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.UI
{
    /// <summary>
    /// Represents one pooled popup and handles its motion, fade, and pool return lifecycle.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FloatingNumber_UMFOSS : MonoBehaviour, IPoolable
    {
        private TextMeshProUGUI textMesh;
        private RectTransform rectTransform;
        private FloatingDamageNumbers_UMFOSS manager;
        private FloatingNumberConfig_UMFOSS.AnimationStyle animationStyle;
        private NumberType currentType;
        private Vector3 startWorldPosition;
        private Vector3 endWorldPosition;
        private Color baseColour;

        public RectTransform RectTransform
        {
            get
            {
                EnsureReferences();
                return rectTransform;
            }
        }

        private void Awake()
        {
            EnsureReferences();
        }

        /// <summary>
        /// Configures the current popup content and motion before playback.
        /// </summary>
        public void Setup(
            FloatingDamageNumbers_UMFOSS owner,
            string displayText,
            Color colour,
            float fontSize,
            NumberType type,
            Vector3 worldPosition,
            FloatingNumberConfig_UMFOSS.AnimationStyle style)
        {
            EnsureReferences();

            manager = owner;
            animationStyle = style;
            currentType = type;
            baseColour = new Color(colour.r, colour.g, colour.b, 1f);
            textMesh.text = displayText;
            textMesh.fontSize = fontSize;
            textMesh.color = baseColour;

            startWorldPosition = worldPosition;
            endWorldPosition = startWorldPosition + Vector3.up * style.floatHeight;
            if (!Mathf.Approximately(style.randomOffset.y, 0f))
            {
                endWorldPosition += Vector3.up * Random.Range(-style.randomOffset.y, style.randomOffset.y);
            }

            manager.ApplyPopupPosition(this, startWorldPosition);
        }

        /// <summary>
        /// Starts the popup animation.
        /// </summary>
        public void Animate()
        {
            StopAllCoroutines();
            StartCoroutine(AnimationCoroutine());
        }

        public void OnSpawnFromPool()
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            StopAllCoroutines();
            if (textMesh != null)
            {
                textMesh.color = baseColour;
            }

            gameObject.SetActive(false);
        }

        private IEnumerator AnimationCoroutine()
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, animationStyle.floatDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);
                float movementValue = animationStyle.movementCurve != null
                    ? animationStyle.movementCurve.Evaluate(normalizedTime)
                    : normalizedTime;

                Vector3 currentWorldPosition = Vector3.LerpUnclamped(startWorldPosition, endWorldPosition, movementValue);
                manager.ApplyPopupPosition(this, currentWorldPosition);

                float alpha = 1f;
                if (normalizedTime >= animationStyle.fadeStartAt)
                {
                    float fadeT = Mathf.InverseLerp(animationStyle.fadeStartAt, 1f, normalizedTime);
                    alpha = animationStyle.fadeCurve != null ? animationStyle.fadeCurve.Evaluate(fadeT) : 1f - fadeT;
                }

                textMesh.color = new Color(baseColour.r, baseColour.g, baseColour.b, alpha);
                yield return null;
            }

            manager.NotifyNumberReturned(currentType);
            ObjectPoolManager_UMFOSS.Instance.Return(gameObject);
        }

        private void EnsureReferences()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshProUGUI>();
            }
        }
    }
}
