using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Defines a scene transition style: fade colour, durations, curves, and
    /// optional loading screen behaviour. Create instances via the Create menu.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTransition", menuName = "UMFOSS/Scene/SceneTransition")]
    public class SceneTransition_UMFOSS : ScriptableObject
    {
        [Header("Fade Out (leaving current scene)")]
        public Color fadeColour = Color.black;
        public float fadeOutDuration = 0.3f;
        public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Fade In (entering new scene)")]
        public float fadeInDuration = 0.3f;
        public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Loading Screen")]
        public bool showLoadingScreen = false;
        public float minimumLoadTime = 0f;
    }
}
