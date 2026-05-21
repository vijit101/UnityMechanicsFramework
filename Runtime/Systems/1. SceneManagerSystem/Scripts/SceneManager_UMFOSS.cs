using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameplayMechanicsUMFOSS.Core;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Centralized scene manager handling async loading, fade transitions,
    /// persistent scenes, and scene stacking (push/pop additive overlays).
    ///
    /// Key design points:
    /// - Uses LoadSceneMode.Additive everywhere so the persistent scene survives.
    /// - allowSceneActivation = false until 90% to prevent half-loaded flashes.
    /// - WaitForSecondsRealtime so transitions work with Time.timeScale = 0.
    /// - Auto-creates a fade canvas on Awake so basic transitions need zero setup.
    /// - Validates scene names against Build Settings before loading.
    /// </summary>
    public class SceneManager_UMFOSS : MonoBehaviour
    {
        // ---------- Inspector ----------
        [Header("Setup")]
        [Tooltip("Name of the scene that holds your singletons. It will never be unloaded.")]
        [SerializeField] private string persistentSceneName;

        [Tooltip("Used when LoadScene/Push/Pop is called without an explicit transition.")]
        [SerializeField] private SceneTransition_UMFOSS defaultTransition;

        [Tooltip("Optional. If unassigned, transitions still work but no loading bar is shown.")]
        [SerializeField] private LoadingScreen_UMFOSS loadingScreen;

        // ---------- Singleton ----------
        public static SceneManager_UMFOSS Instance { get; private set; }

        // ---------- State ----------
        private readonly Stack<string> sceneStack = new Stack<string>();
        private bool isTransitioning;
        private string currentScene;

        // ---------- Auto-created fade canvas ----------
        private Canvas fadeCanvas;
        private Image fadeImage;

        // ===========================================================
        // Unity lifecycle
        // ===========================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateFadeCanvas();

            // Track the scene this manager was instantiated in as the current scene,
            // unless that's the persistent scene itself.
            var active = USceneManager.GetActiveScene().name;
            if (active != persistentSceneName)
            {
                currentScene = active;
                sceneStack.Push(active);
            }

            // EventBus convenience subscriptions so other systems can publish
            // intent without holding a direct reference.
            EventBus.Subscribe<LoadSceneRequest>(OnLoadSceneRequest);
            EventBus.Subscribe<PushSceneRequest>(OnPushSceneRequest);
            EventBus.Subscribe<PopSceneRequest>(OnPopSceneRequest);
            EventBus.Subscribe<ReloadSceneRequest>(OnReloadSceneRequest);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            EventBus.Unsubscribe<LoadSceneRequest>(OnLoadSceneRequest);
            EventBus.Unsubscribe<PushSceneRequest>(OnPushSceneRequest);
            EventBus.Unsubscribe<PopSceneRequest>(OnPopSceneRequest);
            EventBus.Unsubscribe<ReloadSceneRequest>(OnReloadSceneRequest);
        }

        // ===========================================================
        // Public API
        // ===========================================================

        /// <summary>Replaces the current scene with a new one, fading out and in.</summary>
        public void LoadScene(string sceneName, SceneTransition_UMFOSS transition = null)
        {
            if (!GuardCanTransition(sceneName)) return;
            StartCoroutine(LoadSceneRoutine(sceneName, transition ?? defaultTransition));
        }

        /// <summary>Loads a scene additively on top of the current one (overlays, pause menus).</summary>
        public void Push(string sceneName, SceneTransition_UMFOSS transition = null)
        {
            if (!GuardCanTransition(sceneName)) return;
            if (sceneStack.Contains(sceneName))
            {
                Debug.LogWarning($"[SceneManager_UMFOSS] Push ignored — '{sceneName}' is already on the stack.");
                return;
            }
            StartCoroutine(PushRoutine(sceneName, transition ?? defaultTransition));
        }

        /// <summary>Unloads the top scene of the stack and resumes the one beneath.</summary>
        public void Pop(SceneTransition_UMFOSS transition = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneManager_UMFOSS] Pop ignored — a transition is already in progress.");
                return;
            }
            if (sceneStack.Count <= 1)
            {
                Debug.LogWarning("[SceneManager_UMFOSS] Pop ignored — stack depth is too low.");
                return;
            }
            StartCoroutine(PopRoutine(transition ?? defaultTransition));
        }

        /// <summary>Reloads the current scene. Persistent scene is untouched.</summary>
        public void ReloadScene(SceneTransition_UMFOSS transition = null)
        {
            if (string.IsNullOrEmpty(currentScene))
            {
                Debug.LogWarning("[SceneManager_UMFOSS] ReloadScene ignored — no current scene tracked.");
                return;
            }
            if (!GuardCanTransition(currentScene)) return;
            StartCoroutine(ReloadRoutine(transition ?? defaultTransition));
        }

        // Query helpers ---------------------------------------------

        public string GetCurrentScene() => currentScene;
        public bool IsTransitioning() => isTransitioning;
        public bool IsSceneLoaded(string name) => USceneManager.GetSceneByName(name).isLoaded;
        public int GetStackDepth() => sceneStack.Count;

        // ===========================================================
        // Coroutines
        // ===========================================================

        private IEnumerator LoadSceneRoutine(string sceneName, SceneTransition_UMFOSS transition)
        {
            isTransitioning = true;

            // 1. Disable input
            EventBus.Publish(new InputLockEvent { locked = true });

            // 2. OnSceneLoadStart
            EventBus.Publish(new SceneLoadStartEvent { fromScene = currentScene, toScene = sceneName });

            // 3. Fade out
            yield return StartCoroutine(FadeOut(transition));

            // 4. Loading screen
            if (transition.showLoadingScreen && loadingScreen != null)
                loadingScreen.Show();

            // 5. Begin async load — do NOT activate yet
            var asyncOp = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = false;

            // 6. Wait for load to reach 90%
            while (asyncOp.progress < 0.9f)
            {
                EventBus.Publish(new SceneLoadProgressEvent { progress = asyncOp.progress / 0.9f });
                yield return null;
            }

            // Final progress tick at 1.0 so listeners settle.
            EventBus.Publish(new SceneLoadProgressEvent { progress = 1f });

            // 7. Enforce minimum load time (immune to Time.timeScale)
            if (transition.minimumLoadTime > 0f)
                yield return new WaitForSecondsRealtime(transition.minimumLoadTime);

            // 8. Activate new scene FIRST (avoids deadlock with concurrent unload)
            asyncOp.allowSceneActivation = true;
            yield return new WaitUntil(() => asyncOp.isDone);

            // 9. Capture every non-persistent scene currently on the stack — they all need to go.
            var scenesToUnload = new List<string>();
            foreach (var s in sceneStack)
            {
                if (!string.IsNullOrEmpty(s) && s != persistentSceneName)
                    scenesToUnload.Add(s);
            }

            // 10. Update tracking BEFORE unloading so labels reflect the new state immediately.
            sceneStack.Clear();
            sceneStack.Push(sceneName);
            currentScene = sceneName;
            USceneManager.SetActiveScene(USceneManager.GetSceneByName(sceneName));

            // 11. Now unload every previous scene we captured.
            foreach (var s in scenesToUnload)
            {
                yield return USceneManager.UnloadSceneAsync(s);
            }

            // 11. Hide loading screen
            if (transition.showLoadingScreen && loadingScreen != null)
                loadingScreen.Hide();

            // 12. Fade in
            yield return StartCoroutine(FadeIn(transition));

            // 13. Re-enable input
            EventBus.Publish(new InputLockEvent { locked = false });

            // 14. OnSceneLoadComplete
            EventBus.Publish(new SceneLoadCompleteEvent { sceneName = sceneName });

            isTransitioning = false;
        }

        private IEnumerator PushRoutine(string sceneName, SceneTransition_UMFOSS transition)
        {
            isTransitioning = true;
            EventBus.Publish(new InputLockEvent { locked = true });

            // Optional fade out — keep it short for overlays
            yield return StartCoroutine(FadeOut(transition));

            var asyncOp = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = false;

            while (asyncOp.progress < 0.9f)
            {
                EventBus.Publish(new SceneLoadProgressEvent { progress = asyncOp.progress / 0.9f });
                yield return null;
            }
            EventBus.Publish(new SceneLoadProgressEvent { progress = 1f });

            asyncOp.allowSceneActivation = true;
            yield return new WaitUntil(() => asyncOp.isDone);

            // Pushed scene becomes the active one but the previous scene stays loaded.
            sceneStack.Push(sceneName);
            currentScene = sceneName;
            USceneManager.SetActiveScene(USceneManager.GetSceneByName(sceneName));

            yield return StartCoroutine(FadeIn(transition));
            EventBus.Publish(new InputLockEvent { locked = false });

            EventBus.Publish(new ScenePushedEvent { sceneName = sceneName });
            isTransitioning = false;
        }

        private IEnumerator PopRoutine(SceneTransition_UMFOSS transition)
        {
            isTransitioning = true;
            EventBus.Publish(new InputLockEvent { locked = true });

            yield return StartCoroutine(FadeOut(transition));

            var top = sceneStack.Pop();
            yield return USceneManager.UnloadSceneAsync(top);

            // Resume the scene now at the top of the stack.
            currentScene = sceneStack.Count > 0 ? sceneStack.Peek() : null;
            if (!string.IsNullOrEmpty(currentScene))
            {
                USceneManager.SetActiveScene(USceneManager.GetSceneByName(currentScene));
            }

            yield return StartCoroutine(FadeIn(transition));
            EventBus.Publish(new InputLockEvent { locked = false });

            EventBus.Publish(new ScenePoppedEvent { sceneName = top });
            isTransitioning = false;
        }

        private IEnumerator ReloadRoutine(SceneTransition_UMFOSS transition)
        {
            var sceneName = currentScene;
            isTransitioning = true;

            EventBus.Publish(new InputLockEvent { locked = true });
            EventBus.Publish(new SceneLoadStartEvent { fromScene = sceneName, toScene = sceneName });

            yield return StartCoroutine(FadeOut(transition));

            // Unload the existing instance first — we can't have two scenes with the same name loaded.
            yield return USceneManager.UnloadSceneAsync(sceneName);

            // Now load a fresh copy.
            var asyncOp = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncOp.isDone)
            {
                EventBus.Publish(new SceneLoadProgressEvent { progress = Mathf.Clamp01(asyncOp.progress / 0.9f) });
                yield return null;
            }
            EventBus.Publish(new SceneLoadProgressEvent { progress = 1f });

            USceneManager.SetActiveScene(USceneManager.GetSceneByName(sceneName));

            yield return StartCoroutine(FadeIn(transition));

            EventBus.Publish(new InputLockEvent { locked = false });
            EventBus.Publish(new SceneLoadCompleteEvent { sceneName = sceneName });
            EventBus.Publish(new SceneReloadedEvent { sceneName = sceneName });

            isTransitioning = false;
        }

        // ===========================================================
        // Fade
        // ===========================================================

        private IEnumerator FadeOut(SceneTransition_UMFOSS transition)
        {
            if (fadeImage == null || transition == null || transition.fadeOutDuration <= 0f)
                yield break;

            fadeImage.color = new Color(transition.fadeColour.r, transition.fadeColour.g, transition.fadeColour.b, 0f);
            fadeImage.raycastTarget = true;

            float t = 0f;
            while (t < transition.fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(t / transition.fadeOutDuration);
                float a = transition.fadeOutCurve != null ? transition.fadeOutCurve.Evaluate(n) : n;
                var c = transition.fadeColour; c.a = a;
                fadeImage.color = c;
                yield return null;
            }

            var final = transition.fadeColour; final.a = 1f;
            fadeImage.color = final;
        }

        private IEnumerator FadeIn(SceneTransition_UMFOSS transition)
        {
            if (fadeImage == null || transition == null || transition.fadeInDuration <= 0f)
            {
                if (fadeImage != null)
                {
                    var c = fadeImage.color; c.a = 0f; fadeImage.color = c;
                    fadeImage.raycastTarget = false;
                }
                yield break;
            }

            float t = 0f;
            while (t < transition.fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(t / transition.fadeInDuration);
                float a = transition.fadeInCurve != null ? transition.fadeInCurve.Evaluate(n) : n;
                var c = transition.fadeColour; c.a = 1f - a;
                fadeImage.color = c;
                yield return null;
            }

            var final = transition.fadeColour; final.a = 0f;
            fadeImage.color = final;
            fadeImage.raycastTarget = false;
        }

        private void CreateFadeCanvas()
        {
            var canvasGO = new GameObject("UMFOSS_FadeCanvas");
            canvasGO.transform.SetParent(transform, false);

            fadeCanvas = canvasGO.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = short.MaxValue; // Render above everything

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var imgGO = new GameObject("FadeImage");
            imgGO.transform.SetParent(canvasGO.transform, false);

            fadeImage = imgGO.AddComponent<Image>();
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;

            var rt = fadeImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // ===========================================================
        // Validation & guards
        // ===========================================================

        private bool GuardCanTransition(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"[SceneManager_UMFOSS] Ignoring request to load '{sceneName}' — a transition is already in progress.");
                return false;
            }
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneManager_UMFOSS] Scene name is null or empty.");
                return false;
            }
            if (!IsSceneInBuildSettings(sceneName))
            {
                Debug.LogError($"[SceneManager_UMFOSS] Scene '{sceneName}' is not in Build Settings. Add it via File → Build Settings.");
                return false;
            }
            return true;
        }

        private static bool IsSceneInBuildSettings(string sceneName)
        {
            for (int i = 0; i < USceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName) return true;
            }
            return false;
        }

        // ===========================================================
        // EventBus request handlers
        // ===========================================================

        private void OnLoadSceneRequest(LoadSceneRequest e) => LoadScene(e.sceneName, e.transition);
        private void OnPushSceneRequest(PushSceneRequest e) => Push(e.sceneName, e.transition);
        private void OnPopSceneRequest(PopSceneRequest e) => Pop(e.transition);
        private void OnReloadSceneRequest(ReloadSceneRequest e) => ReloadScene(e.transition);
    }

    // ---------- Request events (intent published by other systems) ----------
    public struct LoadSceneRequest { public string sceneName; public SceneTransition_UMFOSS transition; }
    public struct PushSceneRequest { public string sceneName; public SceneTransition_UMFOSS transition; }
    public struct PopSceneRequest { public SceneTransition_UMFOSS transition; }
    public struct ReloadSceneRequest { public SceneTransition_UMFOSS transition; }
}
